using Disboard;

namespace Yacht
{
    class Yacht : Game
    {
        GameState State { get; set; }

        public Yacht(GameInitializeData initData) : base(initData)
            => State = InitialState.New(this, InitialPlayers);

        public override void Start()
            => State = State.OnStart();

        public override void OnGroup(Player player, string message)
            => State = State.OnGroup(player, message);
    }

    class YachtFactory : IGameFactory
    {
        public Game New(GameInitializeData initData) => new Yacht(initData);
        public void OnHelp(Channel channel) => channel.Send("`Yacht`");
    }
}
