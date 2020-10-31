using Disboard;
using DSharpPlus.Entities;

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
        public void OnHelp(DisboardChannel channel)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Vechu")
                .AddField(
                "턴",
                "각 턴은 **주사위 굴리기** 단계와 **셈** 단계로 이루어집니다.\n" +
                "__주사위 굴리기__ 단계에서는 주사위 **2개**를 굴립니다. 이후, 다시 굴리기를 반복할 수 있습니다."
                )
                .AddField(
                "다시 굴리기",
                "두 주사위 눈이 같다면, 마지막으로 굴린 개수 **이하**의 주사위를 다시 굴립니다.\n" +
                "주사위 눈이 같지 않다면, 마지막으로 굴린 개수**보다 적은** 주사위를 다시 굴립니다."
                )
                .AddField(
                "셈",
                "두 주사위의 눈을 **곱한** 값을 자신의 점수에 더하거나 뺍니다."
                )
                .AddField(
                "게임 종료",
                "한 플레이어의 점수가 정확히 **50점**이 되면, 나머지 플레이어가 한 턴씩 진행한 후 게임이 종료됩니다.\n" +
                "점수가 정확히 **50점**인 플레이어들이 승리합니다."
                );
            channel.Send("", embed);
            channel.Send(
                "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R 4, R 66`\n" +
                "`점수를 기록하려면 점수를 더할지 뺄지를 입력하세요. 예시: S +, S -`"
                );
        }
    }
}
