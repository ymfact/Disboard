using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    public abstract class GameContext
    {
        public SendType Send { get; }
        public SendImageType SendImage { get; }
        public SendImagesType SendImages { get; }
        public string GroupURL { get; }
        public Action OnFinish { get; }
        public IReadOnlyList<Player> InitialPlayers { get; }
        /// <summary>
        /// WPF 컨트롤을 사용하여 이미지를 그릴 수 있습니다. EchoVisual.cs를 예제로써 참고하세요.
        /// 사용하려면 Main 함수 윗줄에 [System.STAThread()]를 추가해야 합니다.
        /// </summary>
        public RenderType Render { get; }
        internal ConcurrentQueue<Task> MessageQueue { get; }

        internal protected GameContext(GameInitializeData initData)
        {
            Send = (message, embed) => MessageQueue.Enqueue(initData.Channel.SendMessageAsync(message, embed: embed));
            SendImage = (stream, message, embed) => MessageQueue.Enqueue(initData.Channel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed));
            SendImages = (streams, message, embed) => MessageQueue.Enqueue(initData.Channel.SendMultipleFilesAsync(streams.Enumerate().ToDictionary(_ => $"{_.index}", _ => _.elem), content: message, embed: embed));
            InitialPlayers = initData.Players;
            OnFinish = () => initData.OnFinish(initData.Channel.Id);
            GroupURL = $"https://discord.com/channels/{initData.Channel.GuildId}/{initData.Channel.Id}";
            Render = controlConstructor => initData.Dispatcher.Invoke(() => controlConstructor().Render());
            MessageQueue = initData.MessageQueue;
        }
    }
}
