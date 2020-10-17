using Disboard;

namespace Yacht
{
    abstract class GameState : IGameState
    {
        public readonly Game ctx;

        protected GameState(Game ctx)
            => this.ctx = ctx;

        public abstract IGameState OnGroup(Player player, string message);

        IGameState IGameState.OnGroup(Player player, string message) => OnGroup(player, message);
    }
}
