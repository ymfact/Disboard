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
        public void OnHelp(Channel channel) => channel.Send(
            "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R4, R 66`\n" +
            "`점수를 기록하려면 점수를 더할지 뺄지를 입력하세요. 예시: S +, S -`"
            );
    }
}
