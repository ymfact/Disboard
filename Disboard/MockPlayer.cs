using DSharpPlus.Entities;

namespace Disboard
{
    class MockPlayer : DisboardPlayer
    {
        internal MockPlayer(int index, DiscordMember member, DisboardChannel channel)
            : base(member.Id, $"{index}", member.Mention, channel)
        {
        }
    }
}
