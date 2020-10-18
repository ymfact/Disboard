namespace Xanth
{
    class NullState : IGameState
    {
        public static NullState New => new NullState();
        NullState() { }
        public IGameState OnGroup(Disboard.DisboardPlayer player, string message) => this;
    }
}
