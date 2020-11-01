using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Disboard
{
    using ChannelIdType = UInt64;
    using UserIdType = UInt64;

    /// <summary>
    /// 메시지를 전송합니다.
    /// </summary>
    /// <param name="message">메시지를 작성할 수 있습니다.</param>
    /// <param name="embed">Discord embed를 포함할 수 있습니다. 메시지의 아래에 표시됩니다.</param>
    /// <param name="emoji">Discord emoji를 포함할 수 있습니다. 게시한 메시지에 봇이 리액션을 추가합니다. 너무 많을 경우 느려집니다.</param>
    public delegate void SendType(string message, DiscordEmbed? embed = null, IEnumerable<string>? emoji = null);
    /// <summary>
    /// 한 장의 이미지를 전송합니다.
    /// </summary>
    /// <param name="stream">이미지를 포함하는 스트림입니다. Render 함수를 이용해서 생성할 수 있습니다.</param>
    /// <param name="message">메시지를 작성할 수 있습니다. 이미지의 위에 표시됩니다.</param>
    /// <param name="embed">Discord embed를 포함할 수 있습니다. 이미지의 아래에 표시됩니다.</param>
    /// <param name="emoji">Discord emoji를 포함할 수 있습니다. 게시한 메시지에 봇이 리액션을 추가합니다. 너무 많을 경우 느려집니다.</param>
    public delegate void SendImageType(Stream stream, string? message = null, DiscordEmbed? embed = null, IEnumerable<string>? emoji = null);
    /// <summary>
    /// 여러 장의 이미지를 전송합니다.
    /// </summary>
    /// <param name="streams">이미지를 포함하는 스트림입니다. Render 함수를 이용해서 생성할 수 있습니다.</param>
    /// <param name="message">메시지를 작성할 수 있습니다. 이미지의 위에 표시됩니다.</param>
    /// <param name="embed">Discord embed를 포함할 수 있습니다. 이미지의 아래에 표시됩니다.</param>
    /// <param name="emoji">Discord emoji를 포함할 수 있습니다. 게시한 메시지에 봇이 리액션을 추가합니다. 너무 많을 경우 느려집니다.</param>
    public delegate void SendImagesType(IReadOnlyList<Stream> streams, string? message = null, DiscordEmbed? embed = null, IEnumerable<string>? emoji = null);
    /// <summary>
    /// WPF 컨트롤을 생성하고, 이미지를 그립니다.
    /// </summary>
    /// <param name="controlConstructor">WPF 컨트롤의 생성과 수정은 반드시 이 안에서 이루어져야 합니다.</param>
    /// <returns>PNG 이미지를 포함하는 스트림을 반환합니다.</returns>
    public delegate Stream RenderType(Func<Control> controlConstructor);

    /// <summary>
    /// Online Text-Based Board Game Platform using Discord
    /// </summary>
    /// <typeparam name="GameFactoryType">파라미터가 없는 public 생성자가 있어야 합니다. 기본적인 기능만을 갖고있는 DisboardGameFactory를 사용할 수 있습니다.</typeparam>
    public sealed partial class Disboard<GameFactoryType> where GameFactoryType : IDisboardGameFactory, new()
    {
        bool IsInitialized = false;

        IDisboardGameFactory GameFactory { get; } = new GameFactoryType();
        Dispatcher STADispatcher { get; set; } = null!;
        ConcurrentDictionary<ChannelIdType, DisboardGame> Games { get; } = new ConcurrentDictionary<ChannelIdType, DisboardGame>();
        Dictionary<UserIdType, DisboardGameUsingDM> GamesByUsers { get; } = new Dictionary<UserIdType, DisboardGameUsingDM>();
        DispatcherTimer? TickTimer { get; set; } = null;
        DiscordClient Client { get; set; } = null!;

        /// <summary>
        /// Disboard를 생성합니다.
        /// </summary>
        public Disboard()
        {
            Thread thread = new Thread(() =>
            {
                Application Application = new Application();
                STADispatcher = Application.Dispatcher;
                Application.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Disboard를 실행합니다. 실행을 블락하므로 프로그램의 마지막줄에 있어야 합니다.
        /// </summary>
        /// <param name="token">디스코드 홈페이지에서 토큰을 발급해야 합니다.</param>
        public void Run(string token)
        {
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot
            });
            Client.DebugLogger.LogMessageReceived += LogMessageReceived;
            Client.Ready += Ready;
            Client.GuildCreated += GuildCreated;
            Client.GuildDeleted += GuildDeleted;
            Client.GuildMemberAdded += GuildMemberAdded;
            Client.MessageCreated += MessageCreated;
            Client.GuildMemberUpdated += GuildMemberUpdated;
            Client.UserUpdated += UserUpdated;
            Client.MessageReactionAdded += MessageReactionAdded;

            Client.ConnectAsync().GetAwaiter().GetResult();
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

        void Log(string log)
        {
            Console.WriteLine(log);
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

        async Task<IEnumerable<(DiscordChannel channel, string mockPlayerCount)>> GetDebugChannels(DiscordGuild guild)
        {
            var regex = new Regex("debug([0-9]*)");
            var channels = await guild.GetChannelsAsync();
            return channels.Where(_ => _.Topic != null && regex.IsMatch(_.Topic.ToLower())).Select(_ => (_, regex.Match(_.Topic.ToLower()).Groups[1].Value));
        }

        async Task RunInLockAndProcessMessage(DisboardGame game, Action task, bool gameIsInGameList = true)
        {
            using (await game.Semaphore.LockAsync().ConfigureAwait(false))
            {
                if (gameIsInGameList == false || Games.Values.ToList().Contains(game))
                {
                    await RunAndProcessMessage(task, game.MessageQueue);
                }
            }
        }

        async Task RunAndProcessMessage(Action task, ConcurrentQueue<Func<Task>> messageQueue)
        {
            try
            {
                task();
                while (messageQueue.TryDequeue(out var messageTaskGetter))
                    await messageTaskGetter();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }
    }
}
