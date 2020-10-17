using Disboard;

namespace Xanth
{
    class Xanth : Game
    {
        GameState State { get; set; }

        public Xanth(GameInitializeData initData) : base(initData)
            => State = InitialState.New(this, InitialPlayers);

        public override void Start()
            => State = State.OnStart();

        public override void OnGroup(Player player, string message)
            => State = State.OnGroup(player, message);
    }

    class XanthFactory : IGameFactory
    {
        public Game New(GameInitializeData initData) => new Xanth(initData);

        public void OnHelp(Channel channel)
        {
            channel.Send(
                "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R 446`\n" +
                "`이동하려면 상하좌우에 해당하는 wsad를 입력하세요. 예시: S wasd`\n" +
                "`턴을 넘기려면 S !, 보드에 기록하지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd`\n" +
                "O `No Pair` < P `One Pair` < 3 `Three Straight` < T `Triple` < 2P `Two Pair` < 4 `Four Straight` = X `Xanth`"
                );
        }
    }
}
