using DSharpPlus.Entities;

namespace Disboard
{
    class MockPlayer : Player
    {
        internal MockPlayer(int index, DiscordMember member, Channel channel)
            : base(member.Id, $"{index}", member.Mention, channel)
        {
        }
    }
}
