using Disboard;
using DSharpPlus.Entities;

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

        public override void OnGroup(DisboardPlayer player, string message)
            => State = State.OnGroup(player, message);
    }

    class XanthFactory : IDisboardGameFactory
    {
        public DisboardGame New(DisboardGameInitData initData) => new Xanth(initData);

        public void OnHelp(DisboardChannel channel)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Xanth")
                .WithDescription(
                "2, 4인 게임입니다.\n" +
                "2인 게임은 4x4 게임판을, 4인 게임은 5x5 게임판을 사용합니다."
                )
                .AddField(
                "턴",
                "각 턴은 **주사위 굴리기** 단계와 **이동 및 쓰기** 단계로 이루어집니다.\n" +
                "__주사위 굴리기__ 단계에서는 주사위 **4개**를 굴립니다.이후, 다시 굴리기를 반복할 수 있지만, 마지막으로 굴린 개수보다 **적게** 굴려야 합니다.\n" +
                "__이동 및 쓰기__ 단계에서는 말을 상하좌우로 여러번 이동할 수 있습니다.이동할 수 있는 최대 횟수는 **마지막으로 굴린 주사위 개수**와 같습니다."
                )
                .AddField(
                "쓰기",
                "말을 이동할 때마다, 이동한 칸에 조합을 쓸 수 있습니다.\n" +
                "조합을 쓰면 그 칸은 자신의 소유가 됩니다."
                )
                .AddField(
                "조합",
                "O `No Pair` < P `One Pair` < 3 `Three Straight` < T `Triple` < 2P `Two Pair` < 4 `Four Straight` = X `Xanth`"
                )
                .AddField(
                "쓰기 제한",
                "칸에 조합을 쓰려면, 칸에 적혀있는 조합보다 **같거나 더 큰** 조합의 주사위가 필요합니다.\n" +
                "각 행과 열에는 __금지 숫자__가 존재합니다. 자신의 주사위가 금지 숫자 중 하나 이상을 포함한다면 해당 칸에는 쓸 수 없습니다.\n" +
                "2인 플레이시, 각 플레이어의 첫 이동 직전에, 처음 있던 칸에도 쓸 수 있습니다.이 때는 쓰기 제한을 적용하지 않습니다."
                )
                .AddField(
                "이동 제한",
                "상대가 소유한 칸으로 이동하려면, __쓰기 조건__을 만족해야합니다.\n" +
                "상대의 말이 있는 칸에도 이동이 불가능합니다."
                )
                .AddField(
                "낙오",
                "모든 방향으로 이동이 불가능한 상태에서 턴을 종료하면 패배합니다.\n" +
                "이 경우 마커가 게임에서 제외되지만, 소유하던 칸은 그대로 남습니다."
                )
                .AddField(
                "게임 종료",
                "1. 쓰여지지 않은 칸이 0개가 되면 즉시 게임이 종료됩니다. 소유한 칸이 가장 많은 플레이어들이 승리합니다.\n" +
                "2. 한 플레이어만 남았을 경우, 마지막 남은 플레이어가 승리합니다."
                );
            channel.Send("", embed);
            channel.Send(
                "`리롤하려면 리롤할 주사위를 입력하세요. 예시: R 446`\n" +
                "`이동하려면 상하좌우에 해당하는 wsad를 입력하세요. 예시: S wasd`\n" +
                "`턴을 넘기려면 S !, 보드에 기록하지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd`"
                );
        }
    }
}
