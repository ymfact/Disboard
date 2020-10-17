using Disboard;
using DSharpPlus.Entities;
using System.Linq;

namespace Xanth
{
    class FinalState : GameState
    {
        public static FinalState From(TurnState prev)
        {
            var image = prev.ctx.Render(() => prev.Board.GetBoardGrid(null));

            var scores = prev.Board.Board.Slots.SelectMany(_ => _).GroupBy(_ => _.Owner).Select(_ => (_.Key, _.Count()));
            var highestScore = scores.OrderByDescending(_ => _.Item2).First().Item2;
            var winners = scores.Where(_ => _.Item2 == highestScore).Select(_ => _.Key!.Name);

            var embed = new DiscordEmbedBuilder()
                .AddField(winners.Count() > 1 ? "Winners" : "Winner", string.Join(", ", winners), inline: true);
            prev.ctx.SendImage(image, "@here", embed);

            prev.ctx.OnFinish();

            return new FinalState(prev.ctx);
        }

        FinalState(Game ctx) : base(ctx) { }

    }
}
