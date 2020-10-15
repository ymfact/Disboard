using Disboard;
using System;
using System.Threading.Tasks;

namespace Xanth
{
    abstract class GameState
    {
        public readonly GameContext ctx;

        protected GameState(GameContext context)
            => ctx = context;

        public virtual Task<GameState> OnStart() => Task.FromResult(this);
        public virtual Task<GameState> OnGroup(Player player, string message) => Task.FromResult(this);

        protected static Random Random { get; } = new Random();
    }
}
