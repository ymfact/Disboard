using Disboard;

namespace Xanth
{
    interface IGameState
    {
        IGameState OnGroup(DisboardPlayer player, string message);
    }
}
