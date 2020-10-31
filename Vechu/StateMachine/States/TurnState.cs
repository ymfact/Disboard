﻿using Disboard;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using static Disboard.Macro;

namespace Vechu
{
    partial class TurnState : GameState
    {
        public static TurnState New(DisboardGame ctx, IReadOnlyList<DisboardPlayer> players)
        {
            new VechuFactory().OnHelp(ctx.Channel);

            var board = BoardContext.New(players);
            var turn = TurnContext.New(players.First());
            var next = StartTurn(ctx: ctx, board: board, turn: turn);

            return next;
        }

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

        public override IGameState OnGroup(DisboardPlayer player, string message)
        {
            if (player == Turn.CurrentPlayer)
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
                ctx.Send(W($"리롤할 주사위를 입력하세요. 예시: R 4, R 66"));
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
                ctx.Send(W($"리롤할 주사위를 다시 입력하세요. 예시: R 4, R 66"));
                return this;
            }
        }

        IGameState Submit(string message)
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
                Board.Add(Turn.CurrentPlayer, Turn.Dices[0] * Turn.Dices[1]);
                return ProceedAndStartTurn();
            }
            else if (initials == "-")
            {
                Board.Add(Turn.CurrentPlayer, -Turn.Dices[0] * Turn.Dices[1]);
                return ProceedAndStartTurn();
            }
            else
            {
                ctx.Send(W("잘못 입력했습니다. 두 주사위의 눈을 곱한 값을 더할지 뺄지 선택하세요. 예시: S -, S +"));
                return this;
            }
        }

        IGameState ProceedAndStartTurn()
        {
            DisboardPlayer nextPlayer = Turn.CurrentPlayer.NextPlayer;
            if (Board.ScoreDict[nextPlayer] == 50)
            {
                var image = ctx.Render(() => Board.GetBoardGrid(null));

                var winners = Board.ScoreDict.Where(_ => _.Value == 50).Select(_ => _.Key!.Name);

                var embed = new DiscordEmbedBuilder()
                    .AddField(winners.Count() > 1 ? "Winners" : "Winner", string.Join(", ", winners), inline: true);
                ctx.SendImage(image, "@here", embed);

                ctx.OnFinish();

                return NullState.New;
            }
            else
            {
                return StartTurn(ctx, Board, Turn.Next());
            }
        }

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
            var image = ctx.Render(() => Board.GetBoardGrid((Turn.CurrentPlayer, Turn.Dices)));

            var rerollTexts = Enumerable.Range(0, 2).Reverse().Select(_ => _ < Turn.RemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var rerollString = string.Join(" ", rerollTexts);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.Dices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);

            var embed = new DiscordEmbedBuilder()
                .AddField("Dices", diceString, inline: true)
                .AddField("Reroll", rerollString, inline: true);
            ctx.SendImage(image, $"{Turn.CurrentPlayer.Mention} {Turn.CurrentPlayer.Name}'s turn", embed);
        }
    }
}
