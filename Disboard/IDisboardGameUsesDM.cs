namespace Disboard
{
    public interface IDisboardGameUsesDM : IDisboardGame
    {
        void OnDM(DisboardPlayer author, string message);
    }
}
