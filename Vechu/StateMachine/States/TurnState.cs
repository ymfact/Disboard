using Disboard;
using System.Collections.Generic;
using System.Linq;
using static Disboard.Macro;

namespace Vechu
{
    partial class TurnState : GameState
    {
        static public TurnState From(InitialState prev)
        {
            var board = BoardContext.New(prev.Players);
            var turn = TurnContext.New();
            var next = StartTurn(ctx: prev.ctx, board: board, turn: turn);

            next.ctx.Send("`명령어: R 1, R 66, S +, S -`");
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

        public override GameState OnGroup(Player player, string message)
        {
            if (player == CurrentPlayer)
            {
                var split = message.Split();
                if (split.Length > 0 && split[0].ToLower() == "r")
                {
                    return Reroll(message);
                }
                else if (split.Length > 0 && split[0].ToLower() == "s")
                {
                    return Submit(message);
                }
            }
            return this;
        }
        TurnState Reroll(string message)
        {
            if (Turn.RemainReroll <= 0)
            {
                ctx.Send(W("남은 리롤 기회가 없습니다. 두 주사위의 눈을 곱한 값을 더할지 뺄지 선택하세요. 예시: S -, S +"));
                return this;
            }
            var split = message.Split();
            if (split.Length != 2)
            {
                ctx.Send(W($"리롤할 주사위를 입력하세요. 예시: R 1, R 66"));
                return this;
            }
            try
            {
                var dicesToReroll = split[1].Select(_ => int.Parse(_.ToString())).ToList();
                if (dicesToReroll.Count > Turn.RemainReroll)
                {
                    ctx.Send(W($"두 눈이 같으면 마지막 굴림만큼, 같지 않으면 마지막 굴림보다 적게 리롤할 수 있습니다. 이번에는 {Turn.RemainReroll}개까지 리롤할 수 있습니다."));
                    return this;
                }
                var next = new TurnState(
                   ctx: ctx,
                   board: Board,
                   turn: Turn.Reroll(dicesToReroll)
                   );
                next.PrintTurn();
                return next;
            }
            catch (System.FormatException)
            {
                ctx.Send(W($"리롤할 주사위를 다시 입력하세요. 예시: R 1, R 66"));
                return this;
            }
        }

        GameState Submit(string message)
        {
            var split = message.Split();
            if (split.Length != 2)
            {
                ctx.Send(W("두 주사위의 눈을 곱한 값을 더할지 뺄지 선택하세요. 예시: S -, S +"));
                return this;
            }
            var initials = split[1];
            if (initials == "+")
            {
                Board.Add(CurrentPlayer, Turn.Dices[0] * Turn.Dices[1]);
                return ProceedAndStartTurn();
            }
            else if (initials == "-")
            {
                Board.Add(CurrentPlayer, -Turn.Dices[0] * Turn.Dices[1]);
                return ProceedAndStartTurn();
            }
            else
            {
                ctx.Send(W("잘못 입력했습니다. 두 주사위의 눈을 곱한 값을 더할지 뺄지 선택하세요. 예시: S -, S +"));
                return this;
            }
        }

        GameState ProceedAndStartTurn()
        {
            int newPlayerIndex = Turn.PlayerIndex + 1;
            if (newPlayerIndex >= Board.Players.Count)
            {
                newPlayerIndex = 0;
            }
            if (Board.ScoreDict[Board.Players[newPlayerIndex]] == 50)
            {
                return FinalState.From(this);
            }
            else
            {
                return StartTurn(newPlayerIndex);
            }
        }

        TurnState StartTurn(int nextPlayerIndex)
            => StartTurn(ctx, Board, Turn.Next(nextPlayerIndex));

        static TurnState StartTurn(GameContext ctx, BoardContext board, TurnContext turn)
        {
            TurnState next = new TurnState(
                   ctx: ctx,
                   board: board,
                   turn: turn
                   );
            next.PrintTurn();
            return next;
        }

        void PrintTurn()
        {
            ctx.SendImage(ctx.Render(() => Board.GetBoardGrid((CurrentPlayer, Turn.Dices))));

            var rerollTexts = Enumerable.Range(0, 2).Reverse().Select(_ => _ < Turn.RemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var rerollString = string.Join(" ", rerollTexts);
            var turnIndicator = $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn, Reroll: {rerollString}";
            ctx.Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.Dices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            ctx.Send(diceString);
        }
    }
}
