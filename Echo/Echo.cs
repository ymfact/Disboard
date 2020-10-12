using Disboard;
using System.Threading.Tasks;
using static Disboard.GameInitializer;

class Echo : IGameUsesDM
{
    SendType Send { get; }
    public Echo(GameInitializer initializer) => Send = initializer.Send;
    public Task OnGroup(User.IdType authorId, string message) => Send(message);
    public Task Start() => Task.CompletedTask;
    public Task OnDM(User.IdType authorId, string message, SendType reply) => reply(message);
}
