using Disboard;
using System.Threading.Tasks;

namespace Yacht
{
    class Yacht : Game
    {
        GameState State { get; set; }

        public Yacht(GameInitializeData initData) : base(initData)
            => State = InitialState.New(this, InitialPlayers);

        public override async Task Start()
            => State = await State.OnStart();

        public override async Task OnGroup(Player player, string message)
            => State = await State.OnGroup(player, message);
    }
}
