using Disboard;
using System.Threading.Tasks;

class Echo : IGame
{
    SendType Send { get; }
    public Echo(GameInitializeData initializeData) => Send = initializeData.Send;
    public Task Start() => Send("`Echo Started.`");
    public Task OnGroup(Player player, string message) => Send(message);
}
