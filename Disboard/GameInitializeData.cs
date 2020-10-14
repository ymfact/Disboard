using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Disboard
{
    using ChannelIdType = UInt64;
    public sealed class GameInitializeData
    {
        public GameInitializeData(DiscordChannel channel, IReadOnlyList<Player> players, Action<ChannelIdType> onFinish, Dispatcher dispatcher)
        {
            Send = (message, embed) => channel.SendMessageAsync(message, embed: embed);
            SendImage = (stream, message, embed) => channel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed);
            SendImages = (streams, message, embed) => channel.SendMultipleFilesAsync(streams.Select((stream, index)=>(stream, index)).ToDictionary(_=>$"{_.index}",_ =>_.stream), content: message, embed: embed);
            Players = players;
            OnFinish = () => onFinish(channel.Id);
            GroupURL = $"https://discord.com/channels/{channel.GuildId}/{channel.Id}";
            Render = controlConstructor => dispatcher.Invoke(() => controlConstructor().Render());
        }
        public SendType Send { get; }
        /// <summary>
        /// 사용하려면 프로젝트를 Visual 타입으로 설정해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImageType SendImage { get; }
        /// <summary>
        /// 사용하려면 프로젝트를 Visual 타입으로 설정해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImagesType SendImages { get; }
        public IReadOnlyList<Player> Players { get; }
        public Action OnFinish { get; }
        public string GroupURL { get; }
        /// <summary>
        /// 사용하려면 프로젝트를 Visual 타입으로 설정해야 합니다. README를 참고하세요.
        /// </summary>
        public RenderType Render { get; }
    }
}
