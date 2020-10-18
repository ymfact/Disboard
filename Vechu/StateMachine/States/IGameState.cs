using Disboard;

namespace Vechu
{
    interface IGameState
    {
        IGameState OnGroup(DisboardPlayer player, string message);
    }
}
