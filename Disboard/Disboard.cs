using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Disboard
{
    using ChannelIdType = UInt64;
    using GuildIdType = UInt64;
    using UserIdType = UInt64;

    public delegate void SendType(string message, DiscordEmbed? embed = null);
    public delegate void SendImageType(Stream stream, string? message = null, DiscordEmbed? embed = null);
    public delegate void SendImagesType(IReadOnlyList<Stream> streams, string? message = null, DiscordEmbed? embed = null);
    public delegate Stream RenderType(Func<Control> controlConstructor);

    public sealed class Disboard<GameFactoryType> where GameFactoryType : IGameFactory, new()
    {
        bool IsInitialized = false;

        IGameFactory GameFactory { get; } = new GameFactoryType();
        Application Application { get; } = new Application();
        Dictionary<ChannelIdType, Game> Games { get; } = new Dictionary<ChannelIdType, Game>();
        Dictionary<UserIdType, IGameUsesDM> GamesByUsers { get; } = new Dictionary<UserIdType, IGameUsesDM>();

        public void Run(string token)
        {
            DiscordClient discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot
            });
            discord.DebugLogger.LogMessageReceived += LogMessageReceived;
            discord.Ready += Ready;
            discord.GuildCreated += GuildCreated;
            discord.GuildDeleted += GuildDeleted;
            discord.GuildMemberAdded += GuildMemberAdded;
            discord.MessageCreated += MessageCreated;

            Task.Run(() => discord.ConnectAsync().GetAwaiter().GetResult());
            Application.Run();
        }

        async Task NewGame(DiscordChannel channel, IEnumerable<DiscordUser> users)
        {
            if (users.Count() == 0)
            {
                users = channel.Guild.Members.Where(_ => !_.IsBot && !_.IsCurrent && _.Presence != null && _.Presence.Status == UserStatus.Online);
                await channel.SendMessageAsync("`참가 인원을 입력하지 않는 경우, 현재 온라인인 유저들로 게임이 시작됩니다.`");
            }
            var userIds = users.Select(_ => _.Id);
            var members = channel.Guild.Members.Where(_ => userIds.Contains(_.Id));
            var dMChannels = await Task.WhenAll(members.Select(_ => _.CreateDmChannelAsync()));
            var random = new Random();
            var messageQueue = new ConcurrentQueue<Task>();
            var players = members.Zip(dMChannels).Select(_ => new Player(_.First, _.Second, messageQueue)).OrderBy(_ => random.Next()).ToList();
            var gameInitializeData = new GameInitializeData(channel, players, OnFinish, Application.Dispatcher, messageQueue);
            Game game = GameFactory.New(gameInitializeData);

            OnFinish(channel.Id);
            if (game is IGameUsesDM)
            {
                var gameUsesDM = game as IGameUsesDM;
                foreach (var player in players)
                {
                    if (GamesByUsers.ContainsKey(player.Id))
                    {
                        player.DM("`기존에 진행중이던 게임이 있습니다. 기존 게임에 다시 참여하려면 기존 채널에서 BOT restoredm을 입력하세요.`");
                        GamesByUsers.Remove(player.Id, out _);
                    }
                    GamesByUsers.TryAdd(player.Id, gameUsesDM!);
                }
            }
            Games.TryAdd(channel.Id, game);
            try
            {
                game.Start();
                foreach (var messageTask in messageQueue)
                    await messageTask;
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        void OnFinish(ChannelIdType channelId)
        {
            if (Games.ContainsKey(channelId))
            {
                var players = Games[channelId].InitialPlayers;
                foreach (var player in players)
                    GamesByUsers.Remove(player.Id, out _);
                Games.Remove(channelId, out _);
            }
        }

        void LogMessageReceived(object? sender, DebugLogMessageEventArgs _)
            => Log(_.Message);
        void Log(string log)
            => Console.WriteLine(log);

        Task Ready(ReadyEventArgs _)
        {
            if (IsInitialized)
                return Task.CompletedTask;

            async Task OnReady()
            {
                if (IsInitialized)
                    return;

                var channels = (await Task.WhenAll(_.Client.Guilds.Values.Select(_ => GetDebugChannels(_)))).SelectMany(_ => _);
                await Task.WhenAll(channels.Select(async channel =>
                {
                    await channel.SendMessageAsync("`Disboard started.`\n`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");
                    DiscordUser[] empty = { };
                    await NewGame(channel, empty);
                }));

                IsInitialized = true;
            };

            var task = OnReady().ConfigureAwait(true);
            Task.Run(() => task.GetAwaiter().GetResult());

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
                return PrintDesc(defaultChannel);
            }
        }

        Task GuildCreated(GuildCreateEventArgs _)
            => PrintDesc(_.Guild.GetDefaultChannel());

        Task GuildDeleted(GuildDeleteEventArgs __)
        {
            foreach (DiscordChannel channel in __.Guild.Channels)
                Games.Remove(channel.Id, out _);
            return Task.CompletedTask;
        }
        async Task<IEnumerable<DiscordChannel>> GetDebugChannels(DiscordGuild guild)
        {
            var channels = await guild.GetChannelsAsync();
            return channels.Where(_ => _.Topic != null && _.Topic.ToLower().Contains("debug"));
        }

        Task PrintDesc(DiscordChannel channel)
            => channel.SendMessageAsync("`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");

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
                if (game is IGameUsesDM)
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                    if (player != null)
                    {
                        try
                        {
                            game.OnDM(player, content);
                            foreach (var messageTask in game.MessageQueue)
                                await messageTask;
                        }
                        catch (Exception e)
                        {
                            Log(e.ToString());
                        }
                    }
                }
                else
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다.`");
                }
            }
            if (channel.Type == ChannelType.Text || channel.Type == ChannelType.Group)
            {
                var guild = __.Guild;
                var game = Games.GetValueOrDefault(channel.Id);
                var split = content.Split(" ").ToList();
                if (split.Count > 0 && split[0].ToLower() == "bot")
                {
                    GuildIdType guildId = guild.Id;
                    var mentionedUsers = __.MentionedUsers.Distinct().Where(_ => !_.IsCurrent);
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
                        try
                        {
                            var messageQueue = new ConcurrentQueue<Task>();
                            GameFactory.OnHelp(new Channel(channel, messageQueue));
                            foreach (var messageTask in messageQueue)
                                await messageTask;
                        }
                        catch (Exception e)
                        {
                            Log(e.ToString());
                        }
                    }
                    else if (split.Count > 1 && split[1].ToLower() == "restoredm")
                    {
                        if (game == null)
                        {
                            await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start @참가인원1 @참가인원2...는 어떨까요?`");
                        }
                        else if (false == game is IGameUsesDM)
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
                                var gameUsesDM = game as IGameUsesDM;
                                var member = await guild.GetMemberAsync(authorId);
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
                else
                {
                    if (game != null)
                    {
                        var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                        if (player != null)
                        {
                            try
                            {
                                game.OnGroup(player, content);
                                foreach (var messageTask in game.MessageQueue)
                                    await messageTask;
                            }
                            catch (Exception e)
                            {
                                Log(e.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
