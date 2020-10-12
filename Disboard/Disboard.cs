using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Disboard
{
    using ChannelIdType = UInt64;
    public sealed class Disboard
    {
        private readonly DiscordSocketClient _client;
        private readonly Func<GameInitializer, IGame> _newGame;
        private readonly Dictionary<ChannelIdType, (IGame, IEnumerable<User>)> _games = new Dictionary<ChannelIdType, (IGame, IEnumerable<User>)>();
        private readonly Dictionary<User.IdType, IGameUsesDM> _gamesByUsers = new Dictionary<User.IdType, IGameUsesDM>();

        public Disboard(Func<GameInitializer, IGame> newGame)
        {
            _newGame = newGame;

            var config = new DiscordSocketConfig() { HandlerTimeout = null };
            _client = new DiscordSocketClient(config);
        }

        public async Task Run(string token)
        {
            _client.Log += Log;
            _client.JoinedGuild += JoinedGuild;
            _client.LeftGuild += LeftGuild;
            _client.Ready += Ready;
            _client.UserJoined += UserJoined;
            _client.MessageReceived += MessageReceived;

            // Remember to keep token private or to read it from an 
            // external source! In this case, we are reading the token 
            // from an environment variable. If you do not know how to set-up
            // environment variables, you may find more information on the 
            // Internet or by using other methods such as reading from 
            // a configuration.
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task NewGame(IMessageChannel channel, ulong guildId)
        {
            var discordUsers = (await channel.GetUsersAsync().FlattenAsync()).Where(_ => _.IsBot == false);
            var gameInitializer = new GameInitializer(channel, guildId, discordUsers, () => OnFinish(channel.Id));
            var users = gameInitializer.Users;
            IGame game = _newGame(gameInitializer);

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
            _games.Remove(channel.Id);
            _games.Add(channel.Id, (game, users));
            using (channel.EnterTypingState())
            {
                await game.Start();
            }
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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task Ready()
        {
            var channels = _client.Guilds.SelectMany(guild => GetDebugChannles(guild).Result.Select(channel => (guild.Id, channel)));
            var tasks = channels.Select(async _ =>
            {
                var (guildId, channel) = _;
                await channel.SendMessageAsync("`Disboard started.`");
                await NewGame(channel, guildId);
            });
            await Task.WhenAll(tasks);
        }
        private async Task UserJoined(IGuildUser user)
        {
            var channels = await user.Guild.GetTextChannelsAsync();
            var defaultChannel = await user.Guild.GetDefaultChannelAsync();
            if (channels.Any(_ => _games.ContainsKey(_.Id)))
            {
                await defaultChannel.SendMessageAsync("`게임이 진행중입니다. 게임에 참여하려면 BOT restart로 게임을 다시 시작해야 합니다.`");
            }
            else
            {
                await PrintDesc(defaultChannel);
            }
        }

        private async Task JoinedGuild(IGuild guild)
        {
            var channel = await guild.GetDefaultChannelAsync();
            await PrintDesc(channel);
        }
        private async Task LeftGuild(IGuild guild)
        {
            foreach (IChannel channel in await guild.GetTextChannelsAsync())
            {
                _games.Remove(channel.Id);
            }
        }
        private async Task<IEnumerable<IMessageChannel>> GetDebugChannles(IGuild guild)
            => (await guild.GetTextChannelsAsync()).Where(_ => _.Topic != null && _.Topic.ToLower().Contains("debug"));

        private async Task PrintDesc(IMessageChannel channel)
            => await channel.SendMessageAsync("`BOT start로 게임을 시작할 수 있습니다.`");

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            var userId = message.Author.Id;
            var channel = message.Channel;
            var content = message.Content;
            if(channel is IDMChannel)
            {
                IGameUsesDM? game = _gamesByUsers.GetValueOrDefault(userId);
                if (game is IGameUsesDM)
                {
                    if(game.AcceptsOnDM(userId, content))
                    {
                        using (channel.EnterTypingState())
                        {
                            await game.OnDM(userId, content, _ => channel.SendMessageAsync(_));
                        }
                    }
                }
                else
                {
                    await channel.SendMessageAsync("`진행중인 게임이 없습니다.`");
                }
            }
            else
            {
                var (game, users) = _games.GetValueOrDefault(channel.Id);
                string[] split = content.Split(" ");
                if (split.Length > 0 && split[0].ToLower() == "bot")
                {
                    ulong guildId = message.Reference.GuildId.Value;
                    if (split.Length > 1 && split[1].ToLower() == "start")
                    {
                        if (game == null)
                        {
                            await NewGame(channel, guildId);
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
                            await NewGame(channel, guildId);
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
                                var dMChannel = message.Author.GetOrCreateDMChannelAsync().Result;
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
                        if(game.AcceptsOnGroup(userId, content))
                        {
                            using (channel.EnterTypingState())
                            {
                                await game.OnGroup(userId, content);
                            }
                        }
                    }
                }
            }
        }
    }
}
