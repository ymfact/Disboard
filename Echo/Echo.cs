using Disboard;
class Echo : DisboardGame
{
    public Echo(DisboardGameInitData initData) : base(initData) => Send("`Echo Started.`");
    public override void OnGroup(DisboardPlayer player, string message) => Send(message);
}
