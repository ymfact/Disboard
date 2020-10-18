using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Disboard
{
    public sealed class RealPlayer : DisboardPlayer
    {
        internal RealPlayer(DiscordMember member, DiscordDmChannel dMChannel, ConcurrentQueue<Task> messageQueue)
            : base(member.Id, member.Username, member.Mention, new DisboardChannel(dMChannel, messageQueue))
        {
        }
    }
}
