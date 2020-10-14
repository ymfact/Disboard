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
    using GuildIdType = UInt64;
    using ChannelIdType = UInt64;
    using UserIdType = UInt64;

    public delegate Task SendType(string message, DiscordEmbed? embed = null);
    public delegate Task SendImageType(Stream stream, string? message = null, DiscordEmbed? embed = null);
    public delegate Task SendImagesType(IReadOnlyList<Stream> streams, string? message = null, DiscordEmbed? embed = null);
    public delegate Stream RenderType(Func<Control> controlConstructor);
    public sealed class Disboard
    {
        private Application Application { get; }
        private Func<GameInitializeData, IGame> GameConstructor { get; }
        private readonly ConcurrentDictionary<ChannelIdType, (IGame game, IReadOnlyList<PlayerWithId> players, Semaphore semaphore)> _games = new ConcurrentDictionary<ChannelIdType, (IGame, IReadOnlyList<PlayerWithId>, Semaphore)>();
        private readonly ConcurrentDictionary<UserIdType, (IGameUsesDM game, IReadOnlyList<PlayerWithId> players, Semaphore semaphore)> _gamesByUsers = new ConcurrentDictionary<UserIdType, (IGameUsesDM, IReadOnlyList<PlayerWithId>, Semaphore)>();

        public Disboard(Func<GameInitializeData, IGame> gameConstructor)
        {
            Application = new Application();
            GameConstructor = gameConstructor;
        }

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

        public async Task NewGame(DiscordChannel channel, IEnumerable<DiscordUser> users)
        {
            if(users.Count() == 0)
            {
                users = channel.Guild.Members.Where(_ => !_.IsBot && !_.IsCurrent && _.Presence.Status == UserStatus.Online);
                await channel.SendMessageAsync("`참가 인원을 입력하지 않는 경우, 현재 온라인인 유저들로 게임이 시작됩니다.`");
            }
            var userIds = users.Select(_ => _.Id);
            var members = channel.Guild.Members.Where(_ => userIds.Contains(_.Id));
            var dMChannels = await Task.WhenAll(members.Select(_ => _.CreateDmChannelAsync()));
            var players = members.Zip(dMChannels).Select(_ => new PlayerWithId(_.First, _.Second)).ToList();
            var gameInitializer = new GameInitializeData(channel, players, OnFinish, Application.Dispatcher);
            IGame game = GameConstructor(gameInitializer);
            var semaphore = new Semaphore();

            OnFinish(channel.Id);
            if (game is IGameUsesDM)
            {
                var gameUsesDM = game as IGameUsesDM;
                foreach (var player in players)
                {
                    if (_gamesByUsers.ContainsKey(player.Id))
                    {
                        await player.DM("`기존에 진행중이던 게임이 있습니다. 기존 게임에 다시 참여하려면 기존 채널에서 BOT restoredm을 입력하세요.`");
                        _gamesByUsers.Remove(player.Id, out _);
                    }
                    _gamesByUsers.TryAdd(player.Id, (gameUsesDM!, players, semaphore));
                }
            }
            _games.TryAdd(channel.Id, (game, players, semaphore));
            try
            {
                using (await semaphore.LockAsync())
                {
                    await game.Start();
                }
            }
            catch(Exception e)
            {
                Log(e.ToString());
            }
        }

        public void OnFinish(ChannelIdType channelId)
        {
            if (_games.ContainsKey(channelId))
            {
                var (_, users, _) = _games[channelId];
                foreach (var user in users)
                    _gamesByUsers.Remove(user.Id, out _);
                _games.Remove(channelId, out _);
            }
        }

        private void LogMessageReceived(object? sender, DebugLogMessageEventArgs _)
            => Log(_.Message);
        private void Log(string log)
            => Console.WriteLine(log);

        private Task Ready(ReadyEventArgs _)
        {
            Task.Run(async () =>
            {
                var channels = (await Task.WhenAll(_.Client.Guilds.Values.Select(_ => GetDebugChannels(_)))).SelectMany(_=>_);
                await Task.WhenAll(channels.Select(async channel =>
                {
                    await channel.SendMessageAsync("`Disboard started.`\n`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");
                    DiscordUser[] empty = { };
                    await NewGame(channel, empty);
                }));
            });
            return Task.CompletedTask;
        }
        private Task GuildMemberAdded(GuildMemberAddEventArgs _)
        {
            var defaultChannel = _.Guild.GetDefaultChannel();
            if (_.Guild.Channels.Any(_ => _games.ContainsKey(_.Id)))
            {
                return defaultChannel.SendMessageAsync("`게임이 진행중입니다. 게임에 참여하려면 BOT restart @참가인원1 @참가인원2...로 게임을 다시 시작해야 합니다.`");
            }
            else
            {
                return PrintDesc(defaultChannel);
            }
        }

        private Task GuildCreated(GuildCreateEventArgs _)
            => PrintDesc(_.Guild.GetDefaultChannel());

        private Task GuildDeleted(GuildDeleteEventArgs __)
        {
            foreach (DiscordChannel channel in __.Guild.Channels)
                _games.Remove(channel.Id, out _);
            return Task.CompletedTask;
        }
        private async Task<IEnumerable<DiscordChannel>> GetDebugChannels(DiscordGuild guild)
        {
            var channels = await guild.GetChannelsAsync();
            return channels.Where(_ => _.Topic != null && _.Topic.ToLower().Contains("debug"));
        }

        private Task PrintDesc(DiscordChannel channel)
            => channel.SendMessageAsync("`BOT start @참가인원1 @참가인원2... 로 게임을 시작할 수 있습니다.`");

        private async Task MessageCreated(MessageCreateEventArgs __)
        {
            var channel = __.Channel;
            var author = __.Author;
            var authorId = author.Id;
            var message = __.Message;
            var content = message.Content;

            if (author.IsCurrent)
            {
                return;
            }
            if(message.MessageType != MessageType.Default)
            {
                return;
            }

            if (channel.Type == ChannelType.Private)
            {
                var (game, players, semaphore) = _gamesByUsers.GetValueOrDefault(authorId);
                if (game is IGameUsesDM)
                {
                    var player = players.Where(_ => _.Id == authorId).FirstOrDefault();
                    if(player != null)
                    {
                        try
                        {
                            using (await semaphore.LockAsync())
                            {
                                await game.OnDM(player, content);
                            }
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
            if(channel.Type == ChannelType.Text || channel.Type == ChannelType.Group)
            {
                var guild = __.Guild;
                var (game, players, semaphore) = _games.GetValueOrDefault(channel.Id);
                string[] split = content.Split(" ");
                if (split.Length > 0 && split[0].ToLower() == "bot")
                {
                    GuildIdType guildId = guild.Id;
                    var mentionedUsers = __.MentionedUsers.Distinct().Where(_=>!_.IsCurrent);
                    if (split.Length > 1 && split[1].ToLower() == "start")
                    {
                        if (game == null)
                        {
                            if (mentionedUsers.Count() == split.Length - 2)
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
                    else if (split.Length > 1 && split[1].ToLower() == "restart")
                    {
                        if (game == null)
                        {
                            await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start @참가인원1 @참가인원2...는 어떨까요?`");
                        }
                        else
                        {
                            if (mentionedUsers.Count() == split.Length - 2)
                            {
                                await NewGame(channel, mentionedUsers);
                            }
                            else
                            {
                                await channel.SendMessageAsync("`BOT restart @참가인원1 @참가인원2... 로 게임을 시작합니다.`");
                            }
                        }
                    }
                    else if (split.Length > 1 && split[1].ToLower() == "restoredm")
                    {
                        if (game == null)
                        {
                            await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start @참가인원1 @참가인원2...는 어떨까요?`");
                        }
                        else if(false == game is IGameUsesDM)
                        {
                            await channel.SendMessageAsync("`DM을 사용하지 않는 게임입니다.`");
                        }
                        else
                        {
                            var player = players.Where(_ => _.Id == authorId).FirstOrDefault();
                            if (player == null)
                            {
                                await channel.SendMessageAsync("`게임에 참여하고 있지 않습니다. 게임에 참여하려면 BOT restart @참가인원1 @참가인원2...로 게임을 다시 시작해야 합니다.`");
                            }
                            else
                            {
                                var gameUsesDM = game as IGameUsesDM;
                                var member = await guild.GetMemberAsync(authorId);
                                var dMChannel = await member.CreateDmChannelAsync();
                                if (_gamesByUsers.GetValueOrDefault(authorId).game == game)
                                {
                                    await dMChannel.SendMessageAsync("`이곳에 입력하는 메시지는 해당 채널의 게임으로 전달됩니다.`");
                                }
                                else
                                {
                                    _gamesByUsers.Remove(authorId, out _);
                                    _gamesByUsers.TryAdd(authorId, (gameUsesDM!, players, semaphore));
                                    await dMChannel.SendMessageAsync("`복원되었습니다. 이제부터 이곳에 입력하는 메시지는 해당 채널의 게임으로 전달됩니다.`");
                                }
                            }
                        }
                    }
                    else
                    {
                        await channel.SendMessageAsync("`명령어: BOT start, BOT restart, BOT restoredm`");
                    }
                }
                else
                {
                    if (game != null)
                    {
                        var player = players.Where(_ => _.Id == authorId).FirstOrDefault();
                        if(player != null)
                        {
                            try
                            {
                                using (await semaphore.LockAsync())
                                {
                                    await game.OnGroup(player, content);
                                }
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
