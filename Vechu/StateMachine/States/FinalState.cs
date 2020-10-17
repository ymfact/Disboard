using Disboard;
using DSharpPlus.Entities;
using System.Linq;

namespace Vechu
{
    class FinalState : GameState
    {
        public static FinalState From(TurnState prev)
        {
            var image = prev.ctx.Render(() => prev.Board.GetBoardGrid(null));

            var winners = prev.Board.ScoreDict.Where(_ => _.Value == 50).Select(_ => _.Key!.Name);

            var embed = new DiscordEmbedBuilder()
                .AddField(winners.Count() > 1 ? "Winners" : "Winner", string.Join(", ", winners), inline: true);
            prev.ctx.SendImage(image, "@here", embed);

            prev.ctx.OnFinish();

            return new FinalState(prev.ctx);
        }

        FinalState(Game ctx) : base(ctx) { }

    }
}
