using Disboard;
class Echo : Game
{
    public Echo(GameInitializeData initData) : base(initData) { }
    public override void Start() => Send("`Echo Started.`");
    public override void OnGroup(Player player, string message) => Send(message);
}
