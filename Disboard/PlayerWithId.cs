using DSharpPlus.Entities;
using System;
using System.Linq;

namespace Disboard
{
    using UserIdType = UInt64;
    sealed class PlayerWithId : Player
    {
        internal PlayerWithId(DiscordMember member, DiscordDmChannel dMChannel)
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
        public override string Name { get; }
        public override string Mention { get; }
        public override SendType DM { get; }
        public override string DMURL { get; }
        public SendImageType DMImage { get; }
        public SendImagesType DMImages { get; }
    }
}
