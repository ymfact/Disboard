using Disboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Disboard.Macro;

namespace Yacht
{
    partial class TurnState : GameState
    {
        public static TurnState From(InitialState prev)
        {
            var board = BoardContext.New(prev.Players);
            var turn = TurnContext.New();
            var next = StartTurn(prev.ctx, board, turn);

            next.ctx.Send("`명령어: R 23456, S 4k`");
            return next;
        }

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
            if (Turn.CurrentRemainReroll <= 0)
            {
                ctx.Send(W("남은 리롤 기회가 없습니다. 점수를 적을 항목을 선택하세요. 예시: S 3k"));
                return this;
            }
            var split = message.Split();
            if (split.Length != 2)
            {
                ctx.Send(W("리롤할 주사위를 입력하세요. 예시: R 334"));
                return this;
            }
            try
            {
                var dicesToReroll = split[1].Select(_ => int.Parse(_.ToString()));
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
                ctx.Send(W("리롤할 주사위를 다시 입력하세요. 예시: R 334"));
                return this;
            }
        }

        GameState Submit(string message)
        {
            var split = message.Split();
            var scoreBoard = Board.ScoreBoardDict[CurrentPlayer];
            if (split.Length != 2)
            {
                ctx.Send(W("이니셜을 입력하세요. 예시: S 3k"));
                return this;
            }
            var initial = split[1];
            try
            {
                scoreBoard.Submit(initial, Turn.CurrentDices);
                return ProceedAndStartTurn();
            }
            catch (InvalidOperationException)
            {
                ctx.Send(W("이미 점수를 채운 항목입니다."));
                return this;
            }
            catch (CommandNotFoundException)
            {
                ctx.Send(W("올바른 이니셜을 입력하세요. 예시: S 3k"));
                return this;
            }
        }

        GameState ProceedAndStartTurn()
        {
            if (Board.ScoreBoardDict.Values.All(_ => _.Places.Values.All(_ => _.IsOpen == false)))
            {
                return FinalState.From(this);
            }
            else
            {
                int newPlayerIndex = Turn.CurrentPlayerIndex + 1;
                if (newPlayerIndex >= Board.Players.Count)
                {
                    newPlayerIndex = 0;
                }
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
            ctx.SendImage(ctx.Render(() => Board.GetBoardGrid((CurrentPlayer, Turn.CurrentDices))));

            var checkTexts = Enumerable.Range(0, 3).Reverse().Select(_ => _ < Turn.CurrentRemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var checkString = string.Join(" ", checkTexts);
            var turnIndicator = $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn, Reroll: " + checkString;
            ctx.Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.CurrentDices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            ctx.Send(diceString);
        }
    }

    partial class TurnState
    {
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

        public Player CurrentPlayer => Board.Players.ToList()[Turn.CurrentPlayerIndex];
    }
}
