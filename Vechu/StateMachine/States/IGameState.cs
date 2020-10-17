using Disboard;

namespace Vechu
{
    interface IGameState
    {
        IGameState OnGroup(Player player, string message);
    }
}
