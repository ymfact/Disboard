using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    using GuildIdType = UInt64;
    using UserIdType = UInt64;
    partial class Disboard<GameFactoryType>
    {
        async Task OnBot(DisboardGame? game, DiscordChannel channel, UserIdType authorId, string content, IEnumerable<DiscordUser> mentionedUsers)
        {
            var split = content.Split(" ").ToList();
            GuildIdType guildId = channel.GuildId;
            if (split.Count > 1 && split[1].ToLower() == "start")
            {
                if (game == null)
                {
                    if (mentionedUsers.Count() == split.Count - 2)
                    {
                        await NewGame(channel, mentionedUsers);
                    }
                    else
                    {
                        await channel.SendMessageAsync("`BOT start @참가인원1 @참가인원2... 로 게임을 시작합니다.`");
                    }
                }
                else
                {
                    await channel.SendMessageAsync("`진행중인 게임이 있습니다. BOT restart @참가인원1 @참가인원2...는 어떨까요?`");
                }
            }
            else if (split.Count > 1 && split[1].ToLower() == "restart")
            {
                if (game == null)
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start @참가인원1 @참가인원2...는 어떨까요?`");
                }
                else
                {
                    if (mentionedUsers.Count() == split.Count - 2)
                    {
                        await NewGame(channel, mentionedUsers);
                    }
                    else
                    {
                        await channel.SendMessageAsync("`BOT restart @참가인원1 @참가인원2... 로 게임을 시작합니다.`");
                    }
                }
            }
            else if (split.Count == 2 && split[1].ToLower() == "help")
            {
                if (game == null)
                {
                    await channel.SendMessageAsync("`BOT start @참가인원1 @참가인원2...로 게임을 시작합니다.`");
                }
                else
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                    if (player == null)
                    {
                        await channel.SendMessageAsync("`BOT restart @참가인원1 @참가인원2... 로 게임을 시작합니다.`");
                    }
                }

                var messageQueue = new ConcurrentQueue<Func<Task>>();
                void task() => GameFactory.OnHelp(new DisboardChannel(Client, channel, messageQueue, STADispatcher));
                await RunAndProcessMessage(task, messageQueue);
            }
            else if (split.Count > 1 && split[1].ToLower() == "restoredm")
            {
                if (game == null)
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start @참가인원1 @참가인원2...는 어떨까요?`");
                }
                else if (false == game is DisboardGameUsingDM)
                {
                    await channel.SendMessageAsync("`DM을 사용하지 않는 게임입니다.`");
                }
                else
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                    if (player == null)
                    {
                        await channel.SendMessageAsync("`게임에 참여하고 있지 않습니다. 게임에 참여하려면 BOT restart @참가인원1 @참가인원2...로 게임을 다시 시작해야 합니다.`");
                    }
                    else
                    {
                        var gameUsesDM = game as DisboardGameUsingDM;
                        var member = await channel.Guild.GetMemberAsync(authorId);
                        var dMChannel = await member.CreateDmChannelAsync();
                        if (GamesByUsers.GetValueOrDefault(authorId) == game)
                        {
                            await dMChannel.SendMessageAsync("`이곳에 입력하는 메시지는 해당 채널의 게임으로 전달됩니다.`");
                        }
                        else
                        {
                            GamesByUsers.Remove(authorId, out _);
                            GamesByUsers.TryAdd(authorId, gameUsesDM!);
                            await dMChannel.SendMessageAsync("`복원되었습니다. 이제부터 이곳에 입력하는 메시지는 해당 채널의 게임으로 전달됩니다.`");
                        }
                    }
                }
            }
            else
            {
                await channel.SendMessageAsync("`명령어: BOT start, BOT restart, BOT help, BOT restoredm`");
            }
        }

        async Task OnDM(DisboardGameUsingDM game, DisboardPlayer player, string content)
        {
            var split = content.Split();
            if (game.IsDebug)
            {
                if (split.Length > 0 && int.TryParse(split[0], out int playerIndex) && 0 <= playerIndex && playerIndex < game.InitialPlayers.Count)
                {
                    player = game.InitialPlayers[playerIndex];
                    content = string.Join(' ', split.Skip(1));
                }
                else
                {
                    return;
                }
            }
            await RunInLockAndProcessMessage(game, () => game.OnDM(player, content));
        }

        async Task OnGroup(DisboardGame game, DisboardPlayer player, string content)
        {
            var split = content.Split(" ").ToList();
            if (game.IsDebug)
            {
                if (split.Count > 0 && int.TryParse(split[0], out int playerIndex) && 0 <= playerIndex && playerIndex < game.InitialPlayers.Count)
                {
                    player = game.InitialPlayers[playerIndex];
                    content = string.Join(' ', split.Skip(1));
                }
                else
                {
                    return;
                }
            }
            await RunInLockAndProcessMessage(game, () => game.OnGroup(player, content));
        }

        async Task OnDMReaction(DisboardGameUsingDM game, DisboardPlayer player, DiscordEmoji emoji)
        {
            Action task;
            if (game.IsDebug)
                task = () => game.Channel.Send("`인원을 시뮬레이션 할 때에는 리액션이 지원되지 않습니다.`");
            else
                task = () => game.OnDMReaction(player, emoji);
            await RunInLockAndProcessMessage(game, task);
        }

        async Task OnGroupReaction(DisboardGame game, DisboardPlayer player, DiscordEmoji emoji)
        {
            Action task;
            if (game.IsDebug)
                task = () => game.Channel.Send("`인원을 시뮬레이션 할 때에는 리액션이 지원되지 않습니다.`");
            else
                task = () => game.OnGroupReaction(player, emoji);
            await RunInLockAndProcessMessage(game, task);
        }
    }
}
