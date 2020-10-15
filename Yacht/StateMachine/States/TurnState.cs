﻿using Disboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Yacht.States;
using static Disboard.Macro;

namespace Yacht
{
    partial class TurnState : GameState
    {
        public static async Task<TurnState> From(InitialState prev)
        {
            var board = BoardContext.New(prev.Players);
            var turn = TurnContext.New();
            var next = await StartTurn(prev.ctx, board, turn);

            await next.ctx.Send("`명령어: R 23456, S 4k`");
            return next;
        }

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
            if (Turn.CurrentRemainReroll <= 0)
            {
                await ctx.Send(W("남은 리롤 기회가 없습니다. 점수를 적을 항목을 선택하세요. 예시: S 3k"));
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
                var dicesToReroll = split[1].Select(_ => int.Parse(_.ToString()));
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
            Debug.Assert(CurrentPlayer != null);

            var split = message.Split();
            var scoreBoard = Board.ScoreBoardDict[CurrentPlayer];
            if (split.Length != 2)
            {
                await ctx.Send(W("이니셜을 입력하세요.예시: S 3k"));
                return this;
            }
            var initial = split[1];
            try
            {
                scoreBoard.Submit(initial, Turn.CurrentDices);
                return await ProceedAndStartTurn();
            }
            catch (System.InvalidOperationException)
            {
                await ctx.Send(W("이미 점수를 채운 항목입니다."));
                return this;
            }
            catch (CommandNotFoundException)
            {
                await ctx.Send(W("올바른 이니셜을 입력하세요. 예시: S 3k"));
                return this;
            }
        }

        async Task<GameState> ProceedAndStartTurn()
        {
            if (Board.ScoreBoardDict.Values.All(_ => _.Places.Values.All(_ => _.IsOpen == false)))
            {
                return await FinalState.From(this);
            }
            else
            {
                int newPlayerIndex = Turn.CurrentPlayerIndex;
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
            var currentPlayer = CurrentPlayer;
            Debug.Assert(currentPlayer != null);

            await ctx.SendImage(ctx.Render(() => Board.GetBoardGrid((CurrentPlayer, Turn.CurrentDices))));

            var checkTexts = Enumerable.Range(0, 3).Reverse().Select(_ => _ < Turn.CurrentRemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var checkString = string.Join(" ", checkTexts);
            var turnIndicator = $"{currentPlayer.Mention} {currentPlayer.Name}'s turn, Reroll: " + checkString;
            await ctx.Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.CurrentDices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            await ctx.Send(diceString);
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
