using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Disboard
{
    using UserIdType = UInt64;
    public sealed class RealPlayer : Player
    {
        internal RealPlayer(DiscordMember member, DiscordDmChannel dMChannel, ConcurrentQueue<Task> messageQueue)
            : base(member.Id, member.Username, member.Mention, new Channel(dMChannel, messageQueue))
        {
        }
    }
}
