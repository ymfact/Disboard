using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Disboard
{
    public abstract class Game : IGame
    {
        public Game(GameInitializeData initData)
        {
            IsDebug = initData.IsDebug;
            Channel = new Channel(initData.Channel, initData.MessageQueue);
            InitialPlayers = initData.Players;
            OnFinish = () => initData.OnFinish(initData.Channel.Id);
            MessageQueue = initData.MessageQueue;
            Render = controlConstructor => initData.Dispatcher.Invoke(() => controlConstructor().Render());
        }

        bool IGame.IsDebug => IsDebug;
        internal bool IsDebug { get; }
        ConcurrentQueue<Task> IGame.MessageQueue => MessageQueue;
        internal ConcurrentQueue<Task> MessageQueue { get; }

        public Channel Channel { get; }
        public SendType Send => Channel.Send;
        public SendImageType SendImage => Channel.SendImage;
        public SendImagesType SendImages => Channel.SendImages;
        public string GroupURL => Channel.URL;
        public Action OnFinish { get; }
        public IReadOnlyList<Player> InitialPlayers { get; }
        /// <summary>
        /// WPF 컨트롤을 사용하여 이미지를 그릴 수 있습니다. EchoVisual.cs를 예제로써 참고하세요.
        /// 사용하려면 Main 함수 윗줄에 [System.STAThread()]를 추가해야 합니다.
        /// </summary>
        public RenderType Render { get; }

        public abstract void Start();
        public abstract void OnGroup(Player author, string message);
    }
}
