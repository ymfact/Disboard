namespace Disboard
{
    public abstract class Player
    {
        public abstract string Name { get; }
        public abstract string Mention { get; }
        public abstract SendType DM { get; }
        public abstract string DMURL { get; }
    }
}
