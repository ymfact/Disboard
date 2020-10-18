using Disboard;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static Disboard.Macro;

namespace Yacht
{
    partial class TurnState : GameState
    {
        public static TurnState New(DisboardGame ctx, IReadOnlyList<DisboardPlayer> players)
        {
            new YachtFactory().OnHelp(ctx.Channel);

            var board = BoardContext.New(players);
            var turn = TurnContext.New();
            var next = StartTurn(ctx, board, turn);
            return next;
        }

        public override IGameState OnGroup(DisboardPlayer player, string message)
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
                ctx.Send(W("남은 리롤 기회가 없습니다. 점수를 기록할 항목을 선택하세요. 예시: S 3k"));
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

        IGameState Submit(string message)
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

        IGameState ProceedAndStartTurn()
        {
            if (Board.ScoreBoardDict.Values.All(_ => _.Places.Values.All(_ => _.IsOpen == false)))
            {
                var image = ctx.Render(() => Board.GetBoardGrid(null));

                var highestScore = Board.ScoreBoardDict.Values.Select(_ => _.TotalScore).OrderByDescending(_ => _).First();
                var winners = Board.Players.Where(_ => Board.ScoreBoardDict[_].TotalScore == highestScore).Select(_ => _.Name);

                var embed = new DiscordEmbedBuilder()
                    .AddField(winners.Count() > 1 ? "Winners" : "Winner", string.Join(", ", winners), inline: true);
                ctx.SendImage(image, "@here", embed);

                ctx.OnFinish();

                return NullState.New;
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

        static TurnState StartTurn(DisboardGame ctx, BoardContext board, TurnContext turn)
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
            var image = ctx.Render(() => Board.GetBoardGrid((CurrentPlayer, Turn.CurrentDices)));

            var rerollTexts = Enumerable.Range(0, 3).Reverse().Select(_ => _ < Turn.CurrentRemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var rerollString = string.Join(" ", rerollTexts);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.CurrentDices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);

            var embed = new DiscordEmbedBuilder()
                .AddField("Dices", diceString, inline: true)
                .AddField("Reroll", rerollString, inline: true);
            ctx.SendImage(image, $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn", embed);
        }
    }

    partial class TurnState
    {
        public BoardContext Board { get; }
        TurnContext Turn { get; }

        TurnState(
            DisboardGame ctx,
            BoardContext board,
            TurnContext turn
            ) : base(ctx)
        {
            Board = board;
            Turn = turn;
        }

        public DisboardPlayer CurrentPlayer => Board.Players.ToList()[Turn.CurrentPlayerIndex];
    }
}
