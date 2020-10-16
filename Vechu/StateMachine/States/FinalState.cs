using Disboard;
using System.Linq;

namespace Vechu
{
    class FinalState : GameState
    {
        public static FinalState From(TurnState prev)
        {
            prev.ctx.SendImage(prev.ctx.Render(() => prev.Board.GetBoardGrid(null)));
            var winners = prev.Board.ScoreDict.Where(_ => _.Value == 50).Select(_ => _.Key!.Name);
            var winnerString = "@here ";
            winnerString += winners.Count() > 1 ? "Winners: " : "Winner: ";
            winnerString += string.Join(", ", winners);
            prev.ctx.Send(winnerString);
            prev.ctx.OnFinish();

            return new FinalState(prev.ctx);
        }

        FinalState(GameContext ctx) : base(ctx) { }

    }
}
