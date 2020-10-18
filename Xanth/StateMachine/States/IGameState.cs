using Disboard;

namespace Xanth
{
    interface IGameState
    {
        IGameState OnGroup(Disboard.Player player, string message);
    }
}
