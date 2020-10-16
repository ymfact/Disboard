using Disboard;
using System;
using System.Threading.Tasks;

namespace Yacht
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
