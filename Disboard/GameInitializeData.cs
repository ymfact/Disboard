using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Disboard
{
    using ChannelIdType = UInt64;
    public sealed class GameInitializeData
    {
        internal DiscordChannel Channel { get; }
        internal IReadOnlyList<Player> Players { get; }
        internal Action<ChannelIdType> OnFinish { get; }
        internal Dispatcher Dispatcher { get; }
        internal GameInitializeData(DiscordChannel channel, IReadOnlyList<Player> players, Action<ChannelIdType> onFinish, Dispatcher dispatcher)
        {
            Channel = channel;
            Players = players;
            OnFinish = onFinish;
            Dispatcher = dispatcher;
        }
    }
}
