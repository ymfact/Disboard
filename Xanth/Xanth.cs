using Disboard;
using System.Threading.Tasks;

namespace Xanth
{
    class Xanth : Game
    {
        GameState State { get; set; }

        public Xanth(GameInitializeData initData) : base(initData)
            => State = InitialState.New(this, InitialPlayers);

        public override async Task Start()
            => State = await State.OnStart();

        public override async Task OnGroup(Player player, string message)
            => State = await State.OnGroup(player, message);
    }
}
