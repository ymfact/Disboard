using Disboard;
using System.Collections.Generic;
using System.Linq;
using static Disboard.Macro;

namespace Xanth
{
    partial class TurnState : GameState
    {
        static public TurnState From(InitialState prev)
        {
            new XanthFactory().OnHelp(prev.ctx.Channel);

            var board = BoardContext.New(prev.Players);
            var turn = TurnContext.New();
            var next = StartTurn(ctx: prev.ctx, board: board, turn: turn);

            return next;
        }

        public BoardContext Board { get; }
        TurnContext Turn { get; }

        TurnState(
            Game ctx,
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
                ctx.Send(W("남은 리롤 기회가 없습니다. 이동할 방향을 입력하세요. 턴을 넘기려면 S !, 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
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
                var dicesToReroll = split[1].Select(_ => int.Parse(_.ToString())).ToList();
                if (dicesToReroll.Count > Turn.RemainReroll)
                {
                    ctx.Send(W($"마지막 굴림보다 적게 리롤할 수 있습니다. 이번에는 {Turn.RemainReroll}개까지 리롤할 수 있습니다."));
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
                ctx.Send(W("리롤할 주사위를 다시 입력하세요. 예시: R 334"));
                return this;
            }
        }

        GameState Submit(string message)
        {
            var split = message.Split();
            if (split.Length == 1)
            {
                ctx.Send(W("이동할 방향을 입력하세요. 턴을 넘기려면 S !, 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
                return this;
            }
            if (split.Length > 2)
            {
                ctx.Send(W("이동할 방향을 입력하세요. 턴을 넘기려면 S !, 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
                return this;
            }
            var initials = split[1];
            if (initials == "!")
            {
                return ProceedAndStartTurn();
            }
            if (initials.Where(_ => _ != '!').Count() > Turn.RemainMove)
            {
                ctx.Send(W($"마지막으로 굴린 주사위 개수까지만 이동할 수 있습니다. 이번에는 {Turn.RemainMove}칸까지 이동할 수 있습니다."));
                return this;
            }
            try
            {
                Board.Submit(CurrentPlayer, initials, Turn.Dices);
                return ProceedAndStartTurn();
            }
            catch (MoveProhibitedException)
            {
                ctx.Send(W("이동이 불가능한 곳으로 이동하려 했습니다."));
                return this;
            }
            catch (OutOfBoardException)
            {
                ctx.Send(W("보드 바깥으로 이동하려 했습니다."));
                return this;
            }
            catch (InvalidKeywordException)
            {
                ctx.Send(W("잘못 입력했습니다. 이동할 방향을 입력하세요.턴을 넘기려면 S !, 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
                return this;
            }
        }

        GameState ProceedAndStartTurn()
        {
            if (Board.Board.Slots.All(_ => _.All(_ => _.Owner != null)))
            {
                return FinalState.From(this);
            }
            else
            {
                int newPlayerIndex = Turn.PlayerIndex + 1;
                if (newPlayerIndex >= Board.Players.Count)
                {
                    newPlayerIndex = 0;
                }
                return StartTurn(newPlayerIndex);
            }
        }

        TurnState StartTurn(int nextPlayerIndex)
            => StartTurn(ctx, Board, Turn.Next(nextPlayerIndex));

        static TurnState StartTurn(Game ctx, BoardContext board, TurnContext turn)
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
            ctx.SendImage(ctx.Render(() => Board.GetBoardGrid((Turn.PlayerIndex, GetReachables()))));

            var rank = Rank.Calculate(Turn.Dices);
            var rerollTexts = Enumerable.Range(0, Turn.RemainReroll).Select(_ => ":arrows_counterclockwise:");
            var rerollString = string.Join(" ", rerollTexts);
            var moveTexts = Enumerable.Range(0, Turn.RemainMove).Select(_ => ":arrow_right:");
            var moveString = string.Join(" ", moveTexts);
            var turnIndicator = $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn, ";
            if (Turn.RemainReroll > 0)
                turnIndicator += $"Reroll: {rerollString}, ";
            turnIndicator += $"Move: {moveString}";
            turnIndicator += $"\nDices: {rank.Initial} `{rank.Name}`";
            ctx.Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.Dices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            ctx.Send(diceString);
        }
    }
}
