using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Disboard
{
    using ChannelIdType = UInt64;
    using GuildIdType = UInt64;
    using UserIdType = UInt64;

    public delegate void SendType(string message, DiscordEmbed? embed = null);
    public delegate void SendImageType(Stream stream, string? message = null, DiscordEmbed? embed = null);
    public delegate void SendImagesType(IReadOnlyList<Stream> streams, string? message = null, DiscordEmbed? embed = null);
    public delegate Stream RenderType(Func<Control> controlConstructor);

    public sealed class Disboard<GameFactoryType> where GameFactoryType : IDisboardGameFactory, new()
    {
        bool IsInitialized = false;

        IDisboardGameFactory GameFactory { get; } = new GameFactoryType();
        Application Application { get; } = new Application();
        ConcurrentDictionary<ChannelIdType, IDisboardGame> Games { get; } = new ConcurrentDictionary<ChannelIdType, IDisboardGame>();
        Dictionary<UserIdType, IDisboardGameUsesDM> GamesByUsers { get; } = new Dictionary<UserIdType, IDisboardGameUsesDM>();
        DispatcherTimer? TickTimer { get; set; } = null;

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

        async Task NewDebugGame(DiscordChannel discordChannel, int mockPlayerCount)
        {
            var messageQueue = new ConcurrentQueue<Task>();
            var channel = new DisboardChannel(discordChannel, new ConcurrentQueue<Task>());
            var mockPlayers = Enumerable.Range(0, mockPlayerCount).Select(_ => new MockPlayer(_, discordChannel.Guild.Owner, channel) as DisboardPlayer).ToList();
            await NewGame_(discordChannel, mockPlayers, messageQueue, isDebug: true);
        }
        async Task NewGame(DiscordChannel channel, IEnumerable<DiscordUser> users)
        {
            if (users.Count() == 0)
            {
                users = channel.Guild.Members.Where(_ => !_.IsBot && !_.IsCurrent && _.Presence != null && _.Presence.Status == UserStatus.Online);
                await channel.SendMessageAsync("`참가 인원을 입력하지 않는 경우, 현재 온라인인 유저들로 게임이 시작됩니다.`");
            }
            var random = new Random();
            var userIds = users.Select(_ => _.Id);
            var members = channel.Guild.Members.Where(_ => userIds.Contains(_.Id));
            var dMChannels = await Task.WhenAll(members.Select(_ => _.CreateDmChannelAsync()));
            var messageQueue = new ConcurrentQueue<Task>();
            var players = members.Zip(dMChannels).Select(_ => new RealPlayer(_.First, _.Second, messageQueue) as DisboardPlayer).OrderBy(_ => random.Next()).ToList();
            await NewGame_(channel, players, messageQueue);
        }
        async Task NewGame_(DiscordChannel channel, List<DisboardPlayer> players, ConcurrentQueue<Task> messageQueue, bool isDebug = false)
        {
            var gameInitializeData = new DisboardGameInitData(isDebug, channel, players, OnFinish, Application.Dispatcher, messageQueue);
            DisboardGame game = GameFactory.New(gameInitializeData);

            OnFinish(channel.Id);
            if (game is IDisboardGameUsesDM)
            {
                var gameUsesDM = game as IDisboardGameUsesDM;
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
                game.Start();
                Games.TryAdd(channel.Id, game);
            }, notNeedToEnsureGameIsValid: false);
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
                await Task.WhenAll(channels.Select(async _ =>
                {
                    if (_.mockPlayerCount == "")
                        await NewGame(_.channel, new DiscordUser[] { });
                    else if (int.TryParse(_.mockPlayerCount, out int mockPlayerCount))
                        await NewDebugGame(_.channel, mockPlayerCount);
                }));

                IsInitialized = true;
            };

            Task.Run(() => OnReady().GetAwaiter().GetResult());

            if (TickTimer != null && TickTimer.IsEnabled)
            {
                TickTimer.Tick -= Tick;
                TickTimer.Stop();
            }
            TickTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Dispatcher);
            TickTimer.Interval = TimeSpan.FromSeconds(0.1);
            TickTimer.Tick += Tick;
            TickTimer.Start();

            return Task.CompletedTask;
        }
        void Tick(object? sender, EventArgs e)
        {
            var task = Tick();
            if (false == task.IsCompleted)
            {
                task.GetAwaiter().GetResult();
            }
        }

        async Task Tick()
        {
            foreach (var game in Games.Values)
                await RunInLockAndProcessMessage(game, () => game.OnTick()).ConfigureAwait(false);
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
        async Task<IEnumerable<(DiscordChannel channel, string mockPlayerCount)>> GetDebugChannels(DiscordGuild guild)
        {
            var regex = new Regex("debug([0-9]*)");
            var channels = await guild.GetChannelsAsync();
            return channels.Where(_ => _.Topic != null && regex.IsMatch(_.Topic.ToLower())).Select(_ => (_, regex.Match(_.Topic.ToLower()).Groups[1].Value));
        }

        Task PrintDesc(DiscordChannel channel)
            => channel.SendMessageAsync("`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");

        async Task RunInLockAndProcessMessage(IDisboardGame game, Action task, bool notNeedToEnsureGameIsValid = true)
        {
            using (await game.Semaphore.LockAsync().ConfigureAwait(false))
            {
                if (notNeedToEnsureGameIsValid == false || Games.Values.ToList().Contains(game))
                {
                    try
                    {
                        task();
                        while (game.MessageQueue.TryDequeue(out var messageTask))
                            await messageTask;
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                    }
                }
            }
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
                if (game is IDisboardGameUsesDM)
                {
                    var player = game.InitialPlayers.Where(_ => _.Id == authorId).FirstOrDefault();
                    if (player != null)
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
                            GameFactory.OnHelp(new DisboardChannel(channel, messageQueue));
                            while (messageQueue.TryDequeue(out var messageTask))
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
                        else if (false == game is IDisboardGameUsesDM)
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
                                var gameUsesDM = game as IDisboardGameUsesDM;
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
                            try
                            {
                                await RunInLockAndProcessMessage(game, () => game.OnGroup(player, content));
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
