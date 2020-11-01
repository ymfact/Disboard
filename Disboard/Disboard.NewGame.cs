using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    partial class Disboard<GameFactoryType>
    {
        async Task NewDebugGame(DiscordChannel discordChannel, int mockPlayerCount)
        {
            var messageQueue = new ConcurrentQueue<Func<Task>>();
            var channel = new DisboardChannel(discordChannel, messageQueue, STADispatcher);
            var mockPlayers = Enumerable.Range(0, mockPlayerCount).Select(_ => new MockPlayer(_, discordChannel.Guild.Owner, channel) as DisboardPlayer).ToList();
            foreach (var (index, player) in mockPlayers.Enumerate())
            {
                int nextPlayerIndex = (index == mockPlayers.Count - 1) ? 0 : index + 1;
                player.NextPlayer = mockPlayers[nextPlayerIndex];
            }
            await NewGame_(discordChannel, mockPlayers, messageQueue, isDebug: true);
        }

        async Task NewGame(DiscordChannel channel, IEnumerable<DiscordUser> users)
        {
            if (users.Count() == 0)
            {
                users = channel.Guild.Members.Where(_ => !_.IsBot && !_.IsCurrent && _.Presence != null && _.Presence.Status == UserStatus.Online);
                await channel.SendMessageAsync("`참가 인원을 입력하지 않는 경우, 현재 온라인인 유저들로 게임이 시작됩니다.`");
                if (users.Count() == 0)
                {
                    await channel.SendMessageAsync("`현재 온라인인 유저가 없어 게임을 시작하지 못했습니다.`");
                    return;
                }
            }
            var random = new Random();
            var userIds = users.Select(_ => _.Id);
            var members = channel.Guild.Members.Where(_ => userIds.Contains(_.Id));
            var dMChannels = await Task.WhenAll(members.Select(_ => _.CreateDmChannelAsync()));
            var messageQueue = new ConcurrentQueue<Func<Task>>();
            var players = members.Zip(dMChannels).Select(_ => new RealPlayer(_.First, new DisboardChannel(_.Second, messageQueue, STADispatcher)) as DisboardPlayer).OrderBy(_ => random.Next()).ToList();
            foreach (var (index, player) in players.Enumerate())
            {
                int nextPlayerIndex = (index == players.Count - 1) ? 0 : index + 1;
                player.NextPlayer = players[nextPlayerIndex];
            }
            await NewGame_(channel, players, messageQueue);
        }

        async Task NewGame_(DiscordChannel channel, List<DisboardPlayer> players, ConcurrentQueue<Func<Task>> messageQueue, bool isDebug = false)
        {
            var disboardChannel = new DisboardChannel(channel, messageQueue, STADispatcher);
            var gameInitializeData = new DisboardGameInitData(isDebug, Client, disboardChannel, players, OnFinish, messageQueue);
            DisboardGame game = GameFactory.New(gameInitializeData);

            if (game.IsFinished)
                return;

            OnFinish(channel.Id);
            if (game is DisboardGameUsingDM)
            {
                var gameUsesDM = game as DisboardGameUsingDM;
                foreach (var player in players)
                {
                    if (GamesByUsers.TryGetValue(player.Id, out var existingGame) && existingGame != game)
                    {
                        player.DM("`기존에 진행중이던 게임이 있습니다. 기존 게임에 다시 참여하려면 기존 채널에서 BOT restoredm을 입력하세요.`");
                        GamesByUsers.Remove(player.Id, out _);
                    }
                    GamesByUsers.TryAdd(player.Id, gameUsesDM!);
                }
            }

            await RunInLockAndProcessMessage(game, () =>
            {
                Games.TryAdd(channel.Id, game);
            }, gameIsInGameList: false);
        }
    }
}
