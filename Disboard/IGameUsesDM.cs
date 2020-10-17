namespace Disboard
{
    public interface IGameUsesDM : IGame
    {
        void OnDM(Player author, string message);
    }
}
