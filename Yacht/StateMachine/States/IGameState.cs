using Disboard;

namespace Yacht
{
    interface IGameState
    {
        IGameState OnGroup(DisboardPlayer player, string message);
    }
}
