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
        }
        internal UserIdType Id { get; }
        public string Name { get; }
        public string Mention { get; }
        public Channel Channel { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendType DM => Channel.Send;
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public string DMURL => Channel.URL;
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImageType DMImage => Channel.SendImage;
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 IGameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImagesType DMImages => Channel.SendImages;
    }
}
