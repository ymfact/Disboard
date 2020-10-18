using Disboard;

namespace Xanth
{
    class NullState : IGameState
    {
        public static NullState New => new NullState();
        NullState() { }
        public IGameState OnGroup(Disboard.Player player, string message) => this;
    }
}
