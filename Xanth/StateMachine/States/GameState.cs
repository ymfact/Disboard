using Disboard;

namespace Xanth
{
    abstract class GameState : IGameState
    {
        public readonly DisboardGame ctx;

        protected GameState(DisboardGame ctx)
            => this.ctx = ctx;

        public abstract IGameState OnGroup(Disboard.DisboardPlayer player, string message);

        IGameState IGameState.OnGroup(Disboard.DisboardPlayer player, string message) => OnGroup(player, message);
    }
}
