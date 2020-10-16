using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        internal ConcurrentQueue<Task> MessageQueue { get; }
        internal GameInitializeData(DiscordChannel channel, IReadOnlyList<Player> players, Action<ChannelIdType> onFinish, Dispatcher dispatcher, ConcurrentQueue<Task> messageQueue)
        {
            Channel = channel;
            Players = players;
            OnFinish = onFinish;
            Dispatcher = dispatcher;
            MessageQueue = messageQueue;
        }
    }
}
