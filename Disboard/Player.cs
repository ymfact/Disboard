using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Disboard
{
    using UserIdType = UInt64;
    public sealed class Player
    {
        internal Player(DiscordMember member, DiscordDmChannel dMChannel, ConcurrentQueue<Task> messageQueue)
        {
            Id = member.Id;
            Name = member.Username;
            Mention = member.Mention;
            Channel = new Channel(dMChannel, messageQueue);
            DMURL = "https://discord.com/channels/@me/{dMChannel.Id}";
            DM = (message, embed) => messageQueue.Enqueue(dMChannel.SendMessageAsync(message, embed: embed));
            DMImage = (stream, message, embed) => messageQueue.Enqueue(dMChannel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed));
            DMImages = (streams, message, embed) => messageQueue.Enqueue(dMChannel.SendMultipleFilesAsync(streams.Select((stream, index) => (stream, index)).ToDictionary(_ => $"{_.index}", _ => _.stream), content: message, embed: embed));
        }
        internal UserIdType Id { get; }
        public string Name { get; }
        public string Mention { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public Channel Channel { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendType DM { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public string DMURL { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImageType DMImage { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImagesType DMImages { get; }
    }
}
