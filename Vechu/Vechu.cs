using Disboard;

namespace Vechu
{
    class Vechu : DisboardGame
    {
        IGameState State { get; set; }

        public Vechu(DisboardGameInitData initData) : base(initData)
            => State = TurnState.New(this, InitialPlayers);

        public override void OnGroup(DisboardPlayer player, string message)
            => State = State.OnGroup(player, message);
    }

    class VechuFactory : IDisboardGameFactory
    {
        public DisboardGame New(DisboardGameInitData initData) => new Vechu(initData);
        public void OnHelp(DisboardChannel channel) => channel.Send(
            "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R4, R 66`\n" +
            "`점수를 기록하려면 점수를 더할지 뺄지를 입력하세요. 예시: S +, S -`"
            );
    }
}
