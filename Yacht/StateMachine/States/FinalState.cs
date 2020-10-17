using Disboard;
using DSharpPlus.Entities;
using System.Linq;

namespace Yacht
{
    class FinalState : GameState
    {
        public static FinalState From(TurnState prev)
        {
            var image = prev.ctx.Render(() => prev.Board.GetBoardGrid(null));

            var highestScore = prev.Board.ScoreBoardDict.Values.Select(_ => _.TotalScore).OrderByDescending(_ => _).First();
            var winners = prev.Board.Players.Where(_ => prev.Board.ScoreBoardDict[_].TotalScore == highestScore).Select(_ => _.Name);

            var embed = new DiscordEmbedBuilder()
                .AddField(winners.Count() > 1 ? "Winners" : "Winner", string.Join(", ", winners), inline: true);
            prev.ctx.SendImage(image, "@here", embed);

            prev.ctx.OnFinish();

            return new FinalState(prev.ctx);
        }

        FinalState(Game ctx) : base(ctx) { }

    }
}
