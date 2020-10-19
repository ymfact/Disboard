namespace Disboard
{
    public abstract class DisboardGameUsingDM : DisboardGame
    {
        public DisboardGameUsingDM(DisboardGameInitData initData) : base(initData) { }
        public abstract void OnDM(DisboardPlayer author, string message);
    }
}
