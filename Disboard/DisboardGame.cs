using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Disboard
{
    public abstract class DisboardGame : IDisboardGame
    {
        public DisboardGame(DisboardGameInitData initData)
        {
            IsDebug = initData.IsDebug;
            Channel = new DisboardChannel(initData.Channel, initData.MessageQueue);
            InitialPlayers = initData.Players;
            OnFinish = () => initData.OnFinish(initData.Channel.Id);
            MessageQueue = initData.MessageQueue;
            Render = controlConstructor => initData.Dispatcher.Invoke(() => controlConstructor().Render());
        }

        bool IDisboardGame.IsDebug => IsDebug;
        internal bool IsDebug { get; }
        ConcurrentQueue<Task> IDisboardGame.MessageQueue => MessageQueue;
        internal ConcurrentQueue<Task> MessageQueue { get; }
        Semaphore IDisboardGame.Semaphore => Semaphore;
        internal Semaphore Semaphore { get; } = new Semaphore();

        public DisboardChannel Channel { get; }
        public SendType Send => Channel.Send;
        public SendImageType SendImage => Channel.SendImage;
        public SendImagesType SendImages => Channel.SendImages;
        public string GroupURL => Channel.URL;
        public Action OnFinish { get; }
        public IReadOnlyList<DisboardPlayer> InitialPlayers { get; }
        /// <summary>
        /// WPF 컨트롤을 사용하여 이미지를 그릴 수 있습니다. EchoVisual.cs를 예제로써 참고하세요.
        /// 사용하려면 Main 함수 윗줄에 [System.STAThread()]를 추가해야 합니다.
        /// </summary>
        public RenderType Render { get; }

        public abstract void Start();
        public abstract void OnGroup(DisboardPlayer author, string message);
        /// <summary>
        /// 매 0.1초마다 호출됩니다. 호출 간격은 정확하지 않습니다.
        /// </summary>
        public virtual void OnTick() { }
    }
}
