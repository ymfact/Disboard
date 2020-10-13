using DSharpPlus.Entities;
using System;
using System.Collections.Generic;

namespace Disboard
{
    using ChannelIdType = UInt64;
    public sealed class GameInitializeData
    {
        public GameInitializeData(DiscordChannel channel, IReadOnlyList<Player> players, Action<ChannelIdType> onFinish)
        {
            Send = _ => channel.SendMessageAsync(_);
            Players = players;
            OnFinish = () => onFinish(channel.Id);
            GroupURL = $"https://discord.com/channels/{channel.GuildId}/{channel.Id}";
        }
        public SendType Send { get; }
        public IReadOnlyList<Player> Players { get; }
        public Action OnFinish { get; }
        public string GroupURL { get; }
    }
}
