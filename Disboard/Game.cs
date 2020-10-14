using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    public abstract class Game
    {
        protected SendType Send { get; }
        protected SendImageType SendImage { get; }
        protected SendImagesType SendImages { get; }
        protected IReadOnlyList<Player> Players { get; }
        protected Action OnFinish { get; }
        protected string GroupURL { get; }
        /// <summary>
        /// 사용하려면 프로젝트가 WPF를 사용하도록 설정해야 합니다. README를 참고하세요.
        /// </summary>
        protected RenderType Render { get; }

        public Game(GameInitializeData initData)
        {
            Send = (message, embed) => initData.Channel.SendMessageAsync(message, embed: embed);
            SendImage = (stream, message, embed) => initData.Channel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed);
            SendImages = (streams, message, embed) => initData.Channel.SendMultipleFilesAsync(streams.Enumerate().ToDictionary(_ => $"{_.index}", _ => _.elem), content: message, embed: embed);
            Players = initData.Players;
            OnFinish = () => initData.OnFinish(initData.Channel.Id);
            GroupURL = $"https://discord.com/channels/{initData.Channel.GuildId}/{initData.Channel.Id}";
            Render = controlConstructor => initData.Dispatcher.Invoke(() => controlConstructor().Render());
        }
        public abstract Task Start();
        public abstract Task OnGroup(Player author, string message);
    }
}
