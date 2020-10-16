using Disboard;
using System.Linq;
using System.Threading.Tasks;

namespace Xanth
{
    class FinalState : GameState
    {
        public static FinalState From(TurnState prev)
        {
            prev.ctx.SendImage(prev.ctx.Render(() => prev.Board.GetBoardGrid()));
            var scores = prev.Board.Board.Slots.SelectMany(_ => _).GroupBy(_ => _.Owner).Select(_ => (_.Key, _.Count()));
            var highestScore = scores.OrderByDescending(_ => _.Item2).First().Item2;
            var winners = scores.Where(_ => _.Item2 == highestScore).Select(_ => _.Key!.Name);
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
