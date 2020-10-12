using Discord;
using Discord.Net.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Disboard.GameInitializer;

namespace Disboard
{
    public sealed class GameInitializer
    {
        public delegate Task SendType(string message);
        public GameInitializer(IMessageChannel channel, ulong guildId, IEnumerable<IUser> users, Action onFinish)
        {
            Send = (_) => channel.SendMessageAsync(_);
            Users = users.Select(_ => new User(_));
            OnFinish = onFinish;
            GroupURL = $"https://discord.com/channels/{guildId}/{channel.Id}";
        }
        public SendType Send { get; }
        public IEnumerable<User> Users { get; }
        public Action OnFinish { get; }
        public string GroupURL { get; }
    }

    public sealed class User
    {
        public sealed class IdType : IEquatable<IdType?>
        {
            private IdType(ulong raw) => _raw = raw;
            private readonly ulong _raw;
            public static implicit operator IdType(ulong raw) => new IdType(raw);
            public static bool operator ==(IdType? left, IdType? right) => EqualityComparer<IdType>.Default.Equals(left, right);
            public static bool operator !=(IdType? left, IdType? right) => !(left == right);
            public override bool Equals(object? obj) => Equals(obj as IdType);
            public bool Equals(IdType? other) => other != null && _raw == other._raw;
            public override int GetHashCode() => HashCode.Combine(_raw);
        }
        public User(IUser user)
        {
            var dMChannel = user.GetOrCreateDMChannelAsync().Result;

            Id = user.Id;
            Name = user.Username;
            Mention = user.Mention;
            DM = _ => dMChannel.SendMessageAsync(_);
            DMURL = "https://discord.com/channels/@me/{dMChannel.Id}";
        }
        public IdType Id { get; }
        public string Name { get; }
        public string Mention { get; }
        public SendType DM { get; }
        public string DMURL { get; }
    }
}
