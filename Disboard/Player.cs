using System;
using System.Linq;
using DSharpPlus.Entities;

namespace Disboard
{
    using UserIdType = UInt64;
    public sealed class Player
    {
        internal Player(DiscordMember member, DiscordDmChannel dMChannel)
        {
            Id = member.Id;
            Name = member.Username;
            Mention = member.Mention;
            DM = (message, embed) => dMChannel.SendMessageAsync(message, embed: embed);
            DMURL = "https://discord.com/channels/@me/{dMChannel.Id}";
            DMImage = (stream, message, embed) => dMChannel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed);
            DMImages = (streams, message, embed) => dMChannel.SendMultipleFilesAsync(streams.Select((stream, index) => (stream, index)).ToDictionary(_ => $"{_.index}", _ => _.stream), content: message, embed: embed);
        }
        internal UserIdType Id { get; }
        public string Name { get; }
        public string Mention { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 GameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendType DM { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 GameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public string DMURL { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 GameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImageType DMImage { get; }
        /// <summary>
        /// 플레이어의 DM 입력을 받으려면 GameUsesDM을 사용해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImagesType DMImages { get; }
    }
}
