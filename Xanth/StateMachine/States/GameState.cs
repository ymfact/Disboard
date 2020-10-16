using Disboard;
using System;

namespace Xanth
{
    abstract class GameState
    {
        public readonly GameContext ctx;

        protected GameState(GameContext context)
            => ctx = context;

        public virtual GameState OnStart() => this;
        public virtual GameState OnGroup(Player player, string message) => this;

        protected static Random Random { get; } = new Random();
    }
}
