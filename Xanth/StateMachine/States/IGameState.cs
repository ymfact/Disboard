using Disboard;

namespace Xanth
{
    interface IGameState
    {
        IGameState OnGroup(Player player, string message);
    }
}
