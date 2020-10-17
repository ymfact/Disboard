using Disboard;

namespace Vechu
{
    class Vechu : Game
    {
        GameState State { get; set; }

        public Vechu(GameInitializeData initData) : base(initData)
            => State = InitialState.New(this, InitialPlayers);

        public override void Start()
            => State = State.OnStart();

        public override void OnGroup(Player player, string message)
            => State = State.OnGroup(player, message);
    }

    class VechuFactory : IGameFactory
    {
        public Game New(GameInitializeData initData) => new Vechu(initData);
        public void OnHelp(Channel channel) => channel.Send("`Vechu`");
    }
}
