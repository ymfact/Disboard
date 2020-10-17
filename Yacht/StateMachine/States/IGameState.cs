using Disboard;

namespace Yacht
{
    interface IGameState
    {
        IGameState OnGroup(Player player, string message);
    }
}
