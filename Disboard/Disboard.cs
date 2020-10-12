using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    using ChannelIdType = UInt64;
    public sealed class Disboard
    {
        private readonly Func<GameInitializer, IGame> _newGame;
        private readonly Dictionary<ChannelIdType, (IGame, IEnumerable<User>)> _games = new Dictionary<ChannelIdType, (IGame, IEnumerable<User>)>();
        private readonly Dictionary<User.IdType, IGameUsesDM> _gamesByUsers = new Dictionary<User.IdType, IGameUsesDM>();

        public Disboard(Func<GameInitializer, IGame> newGame)
        {
            _newGame = newGame;
        }

        public async Task Run(string token)
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

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task NewGame(DiscordChannel channel)
        {
            var guild = channel.Guild;
            var discordMembers = guild.Members;
            var gameInitializer = new GameInitializer(channel, discordMembers, OnFinish);
            var users = gameInitializer.Users;
            IGame game = _newGame(gameInitializer);

            OnFinish(channel.Id);
            if(game is IGameUsesDM)
            {
                var gameUsesDM = game as IGameUsesDM;
                foreach (var user in users)
                {
                    if (_gamesByUsers.ContainsKey(user.Id)){
                        await user.DM("`기존에 진행중이던 게임이 있습니다. 기존 게임에 다시 참여하려면 기존 채널에서 BOT restoredm을 입력하세요.`");
                        _gamesByUsers.Remove(user.Id);
                    }
                    _gamesByUsers.Add(user.Id, gameUsesDM!);
                }
            }
            _games.Add(channel.Id, (game, users));
            await game.Start();
        }

        public void OnFinish(ChannelIdType channelId)
        {
            if (_games.ContainsKey(channelId))
            {
                var (game, users) = _games[channelId];
                var userIds = _gamesByUsers.Where(_ => _.Value == game).Select(_ => _.Key);
                foreach (var userId in userIds)
                    _gamesByUsers.Remove(userId);
                _games.Remove(channelId);
            }
        }

        private void LogMessageReceived(object? sender, DebugLogMessageEventArgs _)
        {
            Console.WriteLine(_.Message);
        }

        private Task Ready(ReadyEventArgs _)
        {
            var channels = _.Client.Guilds.Values.SelectMany(_ => GetDebugChannels(_));
            var tasks = channels.Select(async channel =>
            {
                await channel.SendMessageAsync("`Disboard started.`");
                await NewGame(channel);
            });
            Task.Run(() => Task.WhenAll(tasks).GetAwaiter().GetResult());
            return Task.CompletedTask;
        }
        private async Task GuildMemberAdded(GuildMemberAddEventArgs _)
        {
            var defaultChannel = _.Guild.GetDefaultChannel();
            if (_.Guild.Channels.Any(_ => _games.ContainsKey(_.Id)))
            {
                await defaultChannel.SendMessageAsync("`게임이 진행중입니다. 게임에 참여하려면 BOT restart로 게임을 다시 시작해야 합니다.`");
            }
            else
            {
                await PrintDesc(defaultChannel);
            }
        }

        private async Task GuildCreated(GuildCreateEventArgs _)
        {
            await PrintDesc(_.Guild.GetDefaultChannel());
        }
        private Task GuildDeleted(GuildDeleteEventArgs _)
        {
            foreach (DiscordChannel channel in _.Guild.Channels)
                _games.Remove(channel.Id);
            return Task.CompletedTask;
        }
        private IEnumerable<DiscordChannel> GetDebugChannels(DiscordGuild guild)
            => guild.GetChannelsAsync().Result.Where(_ => _.Topic != null && _.Topic.ToLower().Contains("debug"));

        private Task PrintDesc(DiscordChannel channel)
            => channel.SendMessageAsync("`BOT start로 게임을 시작할 수 있습니다.`");

        private async Task MessageCreated(MessageCreateEventArgs _)
        {
            var channel = _.Channel;
            var author = _.Author;
            var userId = author.Id;
            var message = _.Message;
            var content = message.Content;

            if (author.IsBot)
            {
                return;
            }
            if(message.MessageType != MessageType.Default)
            {
                return;
            }

            if (channel.Type == ChannelType.Private)
            {
                IGameUsesDM? game = _gamesByUsers.GetValueOrDefault(userId);
                if (game is IGameUsesDM)
                {
                    await game.OnDM(userId, content, _ => channel.SendMessageAsync(_));
                }
                else
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다.`");
                }
            }
            if(channel.Type == ChannelType.Text || channel.Type == ChannelType.Group)
            {
                var guild = _.Guild;
                var (game, users) = _games.GetValueOrDefault(channel.Id);
                string[] split = content.Split(" ");
                if (split.Length > 0 && split[0].ToLower() == "bot")
                {
                    ulong guildId = guild.Id;
                    if (split.Length > 1 && split[1].ToLower() == "start")
                    {
                        if (game == null)
                        {
                            await NewGame(channel);
                        }
                        else
                        {
                            await channel.SendMessageAsync("`진행중인 게임이 있습니다. BOT restart는 어떨까요?`");
                        }
                    }
                    else if (split.Length > 1 && split[1].ToLower() == "restart")
                    {
                        if (game == null)
                        {
                            await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start는 어떨까요?`");
                        }
                        else
                        {
                            await NewGame(channel);
                        }
                    }
                    else if (split.Length > 1 && split[1].ToLower() == "restoredm")
                    {
                        if (game == null)
                        {
                            await channel.SendMessageAsync("`진행중인 게임이 없습니다. BOT start는 어떨까요?`");
                        }
                        else if(false == game is IGameUsesDM)
                        {
                            await channel.SendMessageAsync("`DM을 사용하지 않는 게임입니다.`");
                        }
                        else
                        {
                            if (users.Any(_ => _.Id == userId))
                            {
                                var gameUsesDM = game as IGameUsesDM;
                                var dMChannel = await guild.GetMemberAsync(userId).Result.CreateDmChannelAsync();
                                if (_gamesByUsers.GetValueOrDefault(userId) == game)
                                {
                                    await dMChannel.SendMessageAsync("`이곳에 입력하는 메시지는 해당 채널의 게임으로 전달됩니다.`");
                                }
                                else
                                {
                                    _gamesByUsers.Remove(userId);
                                    _gamesByUsers.Add(userId, gameUsesDM!);
                                    await dMChannel.SendMessageAsync("`복원되었습니다. 이제부터 이곳에 입력하는 메시지는 해당 채널의 게임으로 전달됩니다.`");
                                }
                            }
                            else
                            {
                                await channel.SendMessageAsync("`게임에 참여하고 있지 않습니다. 게임에 참여하려면 BOT restart로 게임을 다시 시작해야 합니다.`");
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
                        await game.OnGroup(userId, content);
                    }
                }
            }
        }
    }
}
