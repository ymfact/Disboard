using Disboard;
using System;

namespace Xanth
{
    abstract class GameState
    {
        public readonly Game ctx;

        protected GameState(Game ctx)
            => this.ctx = ctx;

        public virtual GameState OnStart() => this;
        public virtual GameState OnGroup(Player player, string message) => this;

        protected static Random Random { get; } = new Random();
    }
}
