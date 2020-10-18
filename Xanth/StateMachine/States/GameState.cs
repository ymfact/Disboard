using Disboard;

namespace Xanth
{
    abstract class GameState : IGameState
    {
        public readonly Game ctx;

        protected GameState(Game ctx)
            => this.ctx = ctx;

        public abstract IGameState OnGroup(Disboard.Player player, string message);

        IGameState IGameState.OnGroup(Disboard.Player player, string message) => OnGroup(player, message);
    }
}
