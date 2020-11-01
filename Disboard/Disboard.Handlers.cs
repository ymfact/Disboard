using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Disboard
{
    partial class Disboard<GameFactoryType>
    {
        void LogMessageReceived(object? sender, DebugLogMessageEventArgs _)
            => Log(_.Message);

        Task GuildMemberUpdated(GuildMemberUpdateEventArgs args)
        {
            var players = Games.Values.SelectMany(_ => _.InitialPlayers.Where(_ => _.Id == args.Member.Id));
            foreach (var player in players)
                player.Nickname = args.NicknameAfter;
            return Task.CompletedTask;
        }

        Task UserUpdated(UserUpdateEventArgs args)
        {
            var players = Games.Values.SelectMany(_ => _.InitialPlayers.Where(_ => _.Id == args.UserBefore.Id));
            foreach (var player in players)
                player.Name = args.UserAfter.Username;
            return Task.CompletedTask;
        }

        Task GuildMemberAdded(GuildMemberAddEventArgs _)
        {
            var defaultChannel = _.Guild.GetDefaultChannel();
            if (_.Guild.Channels.Any(_ => Games.ContainsKey(_.Id)))
            {
                return defaultChannel.SendMessageAsync("`게임이 진행중입니다. 게임에 참여하려면 BOT restart @참가인원1 @참가인원2...로 게임을 다시 시작해야 합니다.`");
            }
            else
            {
                return defaultChannel.SendMessageAsync("`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");
            }
        }

        Task GuildCreated(GuildCreateEventArgs _)
            => _.Guild.GetDefaultChannel().SendMessageAsync("`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");

        Task GuildDeleted(GuildDeleteEventArgs __)
        {
            foreach (DiscordChannel channel in __.Guild.Channels)
                Games.Remove(channel.Id, out _);
            return Task.CompletedTask;
        }

        async Task MessageReactionAdded(MessageReactionAddEventArgs __)
        {
            if (!IsInitialized)
                return;

            var channel = __.Channel;
            var user = __.User;
            var userId = __.User.Id;
            var message = __.Message;
            var emoji = __.Emoji;
            if (user.IsCurrent)
            {
                return;
            }
            if (message.MessageType != MessageType.Default)
            {
                return;
            }
            if (false == message.Author.IsCurrent)
            {
                return;
            }

            if (channel.Type == ChannelType.Private)
            {
                var game = GamesByUsers.GetValueOrDefault(userId);
                if (game is DisboardGameUsingDM)
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == userId).FirstOrDefault();
                    if (player != null)
                    {
                        await OnDMReaction(game, player, emoji);
                    }
                }
                else
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다.`");
                }
            }
            else if (channel.Type == ChannelType.Text)
            {
                var game = Games.GetValueOrDefault(channel.Id);
                if (game != null)
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == userId).FirstOrDefault();
                    if (player != null)
                    {
                        await OnGroupReaction(game, player, emoji);
                    }
                }
            }
        }

        Task Ready(ReadyEventArgs _)
        {
            if (IsInitialized)
                return Task.CompletedTask;

            async Task OnReady()
            {
                if (IsInitialized)
                    return;

                var channels = (await Task.WhenAll(_.Client.Guilds.Values.Select(_ => GetDebugChannels(_)))).SelectMany(_ => _);
                await Task.WhenAll(channels.Select(async _ =>
                {
                    if (_.mockPlayerCount == "")
                        await NewGame(_.channel, new DiscordUser[] { });
                    else if (int.TryParse(_.mockPlayerCount, out int mockPlayerCount))
                        if (mockPlayerCount > 0)
                        {
                            await NewDebugGame(_.channel, mockPlayerCount);
                        }
                        else
                        {
                            await _.channel.SendMessageAsync("`채널 토픽 중 debug 키워드에 문제가 있습니다.`");
                        }
                }));

                IsInitialized = true;
            };

            Task.Run(() => OnReady().GetAwaiter().GetResult());

            if (TickTimer != null && TickTimer.IsEnabled)
            {
                TickTimer.Tick -= Tick;
                TickTimer.Stop();
            }
            TickTimer = new DispatcherTimer(DispatcherPriority.Background, STADispatcher)
            {
                Interval = TimeSpan.FromSeconds(0.1),
            };
            TickTimer.Tick += Tick;
            TickTimer.Start();

            return Task.CompletedTask;
        }

        async Task MessageCreated(MessageCreateEventArgs __)
        {
            if (!IsInitialized)
                return;

            var channel = __.Channel;
            var author = __.Author;
            var authorId = author.Id;
            var message = __.Message;
            var content = string.Join(' ', message.Content.Split(' ').Where(_ => _ != ""));

            if (author.IsCurrent)
            {
                return;
            }
            if (message.MessageType != MessageType.Default)
            {
                return;
            }

            if (channel.Type == ChannelType.Private)
            {
                var game = GamesByUsers.GetValueOrDefault(authorId);
                if (game is DisboardGameUsingDM)
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                    if (player != null)
                    {
                        await OnDM(game, player, content);
                    }
                }
                else
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다.`");
                }
            }
            else if (channel.Type == ChannelType.Text)
            {
                var game = Games.GetValueOrDefault(channel.Id);
                var split = content.Split(" ").ToList();
                if (split.Count > 0 && split[0].ToLower() == "bot")
                {
                    var mentionedUsers = __.MentionedUsers.Distinct().Where(_ => !_.IsCurrent);
                    await OnBot(game, channel, authorId, content, mentionedUsers);
                }
                else
                {
                    if (game != null)
                    {
                        var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                        if (player != null)
                        {
                            await OnGroup(game, player, content);
                        }
                    }
                }
            }
        }
    }
}
