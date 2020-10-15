using Disboard;
using System.Linq;
using System.Threading.Tasks;

namespace Yacht
{
    class FinalState : GameState
    {
        public static async Task<FinalState> From(TurnState prev)
        {
            await prev.ctx.SendImage(prev.ctx.Render(() => prev.Board.GetBoardGrid(null)));
            var highestScore = prev.Board.ScoreBoardDict.Values.Select(_ => _.TotalScore).OrderByDescending(_ => _).First();
            var winners = prev.Board.Players.Where(_ => prev.Board.ScoreBoardDict[_].TotalScore == highestScore).Select(_ => _.Name);
            var winnerString = "@here ";
            winnerString += winners.Count() > 1 ? "Winners: " : "Winner: ";
            winnerString += string.Join(", ", winners);
            await prev.ctx.Send(winnerString);
            prev.ctx.OnFinish();

            return new FinalState(prev.ctx);
        }

        FinalState(GameContext ctx) : base(ctx) { }

    }
}
