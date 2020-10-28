using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Disboard
{
    using ChannelIdType = UInt64;

    /// <summary>
    /// 게임의 초기화에 필요한 데이터들입니다.
    /// </summary>
    public sealed class DisboardGameInitData
    {
        internal bool IsDebug { get; }
        internal DiscordChannel Channel { get; }
        internal IReadOnlyList<DisboardPlayer> Players { get; }
        internal Action<ChannelIdType> OnFinish { get; }
        internal Dispatcher Dispatcher { get; }
        internal ConcurrentQueue<Func<Task>> MessageQueue { get; }
        internal DisboardGameInitData(bool isDebug, DiscordChannel channel, IReadOnlyList<DisboardPlayer> players, Action<ChannelIdType> onFinish, Dispatcher dispatcher, ConcurrentQueue<Func<Task>> messageQueue)
        {
            IsDebug = isDebug;
            Channel = channel;
            Players = players;
            OnFinish = onFinish;
            Dispatcher = dispatcher;
            MessageQueue = messageQueue;
        }
    }
}
