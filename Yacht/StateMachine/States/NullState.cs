using Disboard;

namespace Yacht
{
    class NullState : IGameState
    {
        public static NullState New => new NullState();
        NullState() { }
        public IGameState OnGroup(Player player, string message) => this;
    }
}
