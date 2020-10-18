namespace Xanth
{
    interface IGameState
    {
        IGameState OnGroup(Disboard.DisboardPlayer player, string message);
    }
}
