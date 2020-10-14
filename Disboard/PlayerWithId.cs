using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Windows.Threading;

namespace Disboard
{
    using UserIdType = UInt64;
    sealed class PlayerWithId : Player
    {
        public PlayerWithId(DiscordMember member, DiscordDmChannel dMChannel)
        {
            Id = member.Id;
            Name = member.Username;
            Mention = member.Mention;
            DM = (message, embed) => dMChannel.SendMessageAsync(message, embed: embed);
            DMURL = "https://discord.com/channels/@me/{dMChannel.Id}";
            DMImage = (stream, message, embed) => dMChannel.SendFileAsync(stream, file_name: "image.png", content: message, embed: embed);
            DMImages = (streams, message, embed) => dMChannel.SendMultipleFilesAsync(streams.Select((stream, index) => (stream, index)).ToDictionary(_ => $"{_.index}", _ => _.stream), content: message, embed: embed);
        }
        public UserIdType Id { get; }
        public override string Name { get; }
        public override string Mention { get; }
        public override SendType DM { get; }
        public override string DMURL { get; }
        /// <summary>
        /// 사용하려면 프로젝트를 Visual 타입으로 설정해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImageType DMImage { get; }
        /// <summary>
        /// 사용하려면 프로젝트를 Visual 타입으로 설정해야 합니다. README를 참고하세요.
        /// </summary>
        public SendImagesType DMImages { get; }
    }
}
