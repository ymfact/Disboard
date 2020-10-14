using Disboard;
using System.Threading.Tasks;

class Echo : Game
{
    public Echo(GameInitializeData initData) : base(initData) { }
    public override Task Start() => Send("`Echo Started.`");
    public override Task OnGroup(Player player, string message) => Send(message);
}
