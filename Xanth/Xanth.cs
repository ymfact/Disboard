using Disboard;

namespace Xanth
{
    class Xanth : DisboardGame
    {
        IGameState State { get; set; } = NullState.New;

        public Xanth(DisboardGameInitData initData) : base(initData)
        {
            if (InitialPlayers.Count == 2 || InitialPlayers.Count == 4)
            {
                State = TurnState.New(this, InitialPlayers);
            }
            else
            {
                Send("`Xanth는 2인, 4인으로만 플레이 가능합니다.`");
                OnFinish();
            }
        }

        public override void OnGroup(Disboard.DisboardPlayer player, string message)
            => State = State.OnGroup(player, message);
    }

    class XanthFactory : IDisboardGameFactory
    {
        public DisboardGame New(DisboardGameInitData initData) => new Xanth(initData);

        public void OnHelp(DisboardChannel channel)
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
