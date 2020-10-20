using DSharpPlus.Entities;
namespace Disboard
{
    class RealPlayer : DisboardPlayer
    {
        internal RealPlayer(DiscordMember member, DisboardChannel dMChannel)
            : base(member.Id, member.Username, member.Nickname, member.Mention, dMChannel)
        {
        }
    }
}
