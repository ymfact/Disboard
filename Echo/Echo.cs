using Disboard;
class Echo : Game
{
    public Echo(GameInitializeData initData) : base(initData) { }
    public override void Start() => Send("`Echo Started.`");
    public override void OnGroup(Player player, string message) => Send(message);
}

class EchoFactory : IGameFactory
{
    public Game New(GameInitializeData initData) => new Echo(initData);
    public void OnHelp(Channel channel) => channel.Send("`Echo`");
}
