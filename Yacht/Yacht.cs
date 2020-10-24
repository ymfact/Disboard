using Disboard;

namespace Yacht
{
    class Yacht : DisboardGame
    {
        IGameState State { get; set; } = NullState.New;

        public Yacht(DisboardGameInitData initData) : base(initData)
            => State = TurnState.New(this, InitialPlayers);

        public override void OnGroup(DisboardPlayer player, string message)
            => State = State.OnGroup(player, message);
    }

    class YachtFactory : IDisboardGameFactory
    {
        public DisboardGame New(DisboardGameInitData initData) => new Yacht(initData);
        public void OnHelp(DisboardChannel channel)
        {
            channel.Send(
                "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R 446`\n" +
                "`점수를 기록하려면 이니셜을 입력하세요. 예시: S 3k`"
                );
        }
    }
}
