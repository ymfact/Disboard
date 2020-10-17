namespace Disboard
{
    public interface IGameFactory
    {
        Game New(GameInitializeData initData);

        void OnHelp(Channel channel);
    }
}
