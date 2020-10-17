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
        public void OnHelp(Channel channel)
        {
            channel.Send(
                "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R 446`\n" +
                "`점수를 기록하려면 이니셜을 입력하세요. 예시: S 3k`"
                );
        }
    }
}
