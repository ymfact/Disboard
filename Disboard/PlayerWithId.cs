using DSharpPlus.Entities;
using System;

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
            DM = _ => dMChannel.SendMessageAsync(_);
            DMURL = "https://discord.com/channels/@me/{dMChannel.Id}";
        }
        public UserIdType Id { get; }
        public override string Name { get; }
        public override string Mention { get; }
        public override SendType DM { get; }
        public override string DMURL { get; }
    }
}
