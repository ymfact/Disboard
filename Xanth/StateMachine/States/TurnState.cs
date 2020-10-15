using Disboard;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Disboard.Macro;

namespace Xanth
{
    class TurnState : GameState
    {
        static public async Task<TurnState> From(InitialState prev)
        {

            var board = BoardContext.New(prev.Players);
            var turn = TurnContext.New();
            var next = await StartTurn(ctx: prev.ctx, board: board, turn: turn);

            await next.ctx.Send("`명령어: R 234, S wasd  이동 후 쓰지 않으려면 !를 입력합니다. 예시: S w!asd`");
            return next;
        }

        public BoardContext Board { get; }
        TurnContext Turn { get; }

        TurnState(
            GameContext ctx,
            BoardContext board,
            TurnContext turn
            ) : base(ctx)
        {
            Board = board;
            Turn = turn;
        }

        public Player CurrentPlayer => Board.Players.ToList()[Turn.PlayerIndex];

        public override async Task<GameState> OnGroup(Player player, string message)
        {
            if (player == CurrentPlayer)
            {
                var split = message.Split();
                if (split.Length > 0 && split[0].ToLower() == "r")
                {
                    return await Reroll(message);
                }
                else if (split.Length > 0 && split[0].ToLower() == "s")
                {
                    return await Submit(message);
                }
            }
            return this;
        }
        async Task<TurnState> Reroll(string message)
        {
            if (Turn.RemainReroll <= 0)
            {
                await ctx.Send(W("남은 리롤 기회가 없습니다. 이동할 방향을 입력하세요. 예시: S wasd"));
                return this;
            }
            var split = message.Split();
            if (split.Length != 2)
            {
                await ctx.Send(W("리롤할 주사위를 입력하세요. 예시: R 334"));
                return this;
            }
            try
            {
                var dicesToReroll = split[1].Select(_ => int.Parse(_.ToString())).ToList();
                if (dicesToReroll.Count > Turn.RemainReroll)
                {
                    await ctx.Send(W($"마지막 굴림보다 적게 리롤할 수 있습니다. 이번에는 {Turn.RemainReroll}개까지 리롤할 수 있습니다."));
                    return this;
                }
                var next = new TurnState(
                   ctx: ctx,
                   board: Board,
                   turn: Turn.Reroll(dicesToReroll)
                   );
                await next.PrintTurn();
                return next;
            }
            catch (System.FormatException)
            {
                await ctx.Send(W("리롤할 주사위를 다시 입력하세요. 예시: R 334"));
                return this;
            }
        }

        async Task<GameState> Submit(string message)
        {
            var split = message.Split();
            if(split.Length == 1)
            {
                await ctx.Send(W("이동할 방향을 입력하세요. 예시: S wasd  턴을 마치려면 S !를 입력하세요."));
                return this;
            }
            if (split.Length > 2)
            {
                await ctx.Send(W("이동할 방향을 입력하세요. 예시: S wasd"));
                return this;
            }
            var initials = split[1];
            if (initials == "!")
            {
                return await ProceedAndStartTurn();
            }
            if (initials.Where(_ => _ != '!').Count() > Turn.RemainMove)
            {
                await ctx.Send(W($"마지막으로 굴린 주사위 개수까지만 이동할 수 있습니다. 이번에는 {Turn.RemainMove}칸까지 이동할 수 있습니다."));
                return this;
            }
            try
            {
                Board.Submit(CurrentPlayer, initials, Turn.Dices);
                return await ProceedAndStartTurn();
            }
            catch (MoveProhibitedException)
            {
                await ctx.Send(W("이동이 불가능한 곳으로 이동하려 했습니다."));
                return this;
            }
            catch (OutOfBoardException)
            {
                await ctx.Send(W("보드 바깥으로 이동하려 했습니다."));
                return this;
            }
            catch (InvalidOperationException)
            {
                await ctx.Send(W("잘못 입력했습니다. 이동할 방향을 입력하세요. 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
                return this;
            }
        }

        async Task<GameState> ProceedAndStartTurn()
        {
            if (Board.Board.Slots.All(_ => _.All(_ => _.Owner != null)))
            {
                return await FinalState.From(this);
            }
            else
            {
                int newPlayerIndex = Turn.PlayerIndex + 1;
                if (newPlayerIndex >= Board.Players.Count)
                {
                    newPlayerIndex = 0;
                }
                return await StartTurn(newPlayerIndex);
            }
        }

        Task<TurnState> StartTurn(int nextPlayerIndex)
            => StartTurn(ctx, Board, Turn.Next(nextPlayerIndex));

        static async Task<TurnState> StartTurn(GameContext ctx, BoardContext board, TurnContext turn)
        {
            TurnState next = new TurnState(
                   ctx: ctx,
                   board: board,
                   turn: turn
                   );
            await next.PrintTurn();
            return next;
        }

        async Task PrintTurn()
        {
            await ctx.SendImage(ctx.Render(() => Board.GetBoardGrid()));

            var rerollTexts = Enumerable.Range(0, Turn.RemainReroll).Select(_ => ":arrows_counterclockwise:");
            var rerollString = string.Join(" ", rerollTexts);
            var moveTexts = Enumerable.Range(0, Turn.RemainMove).Select(_ => ":arrow_right:");
            var moveString = string.Join(" ", moveTexts);
            var turnIndicator = $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn, ";
            if(Turn.RemainReroll > 0)
                turnIndicator += $"Reroll: {rerollString} or ";
            turnIndicator += $"Move: {moveString}";
            await ctx.Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.Dices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            await ctx.Send(diceString);
        }
    }
}
