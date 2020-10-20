using DSharpPlus.Entities;
namespace Disboard
{
    public sealed class RealPlayer : DisboardPlayer
    {
        internal RealPlayer(DiscordMember member, DisboardChannel dMChannel)
            : base(member.Id, member.Username, member.Mention, dMChannel)
        {
        }
    }
}
