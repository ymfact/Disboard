namespace Disboard
{
    public interface IDisboardGameFactory
    {
        DisboardGame New(DisboardGameInitData initData);

        void OnHelp(DisboardChannel channel);
    }
}
