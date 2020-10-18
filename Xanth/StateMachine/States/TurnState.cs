using Disboard;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using static Disboard.Macro;

namespace Xanth
{
    class TurnState : GameState
    {
        static public TurnState New(DisboardGame ctx, IReadOnlyList<Disboard.DisboardPlayer> players)
        {
            new XanthFactory().OnHelp(ctx.Channel);

            var board = BoardContext.New(players);
            var turn = TurnContext.New(board);
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

        public Player CurrentPlayer => Board.Players[Turn.PlayerIndex];

        public override IGameState OnGroup(Disboard.DisboardPlayer player, string message)
        {
            if (player == CurrentPlayer.Disboard)
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
                   turn: Turn.Reroll(Board, dicesToReroll)
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
            if (split.Length != 2)
            {
                ctx.Send(W("이동할 방향을 입력하세요. 턴을 넘기려면 S !, 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
                return this;
            }
            var initials = split[1];
            if (initials == "!")
            {
                if (Turn.IsStuckInThisTurn && Turn.RemainReroll > 0)
                {
                    ctx.Send(W("남은 리롤 기회가 있습니다. 이동할 방향을 입력하세요. 이동 후 보드에 쓰지 않으려면 문자 뒤에 !를 입력합니다. 예시: S w!asd"));
                }
                else
                {
                    return ProceedAndStartTurn();
                }
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
            catch (MoveAfterGameEndException)
            {
                ctx.Send(W("칸이 모두 채워지면 즉시 게임이 종료되어 추가로 이동할 수 없습니다."));
                return this;
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

        IGameState ProceedAndStartTurn()
        {
            if (Board.Board.Slots.All(_ => _.All(_ => _.Owner != null)))
            {
                var scores = Board.Board.Slots.SelectMany(_ => _).GroupBy(_ => _.Owner).Select(_ => (_.Key, _.Count()));
                var highestScore = scores.OrderByDescending(_ => _.Item2).First().Item2;
                var winners = scores.Where(_ => _.Item2 == highestScore).Select(_ => _.Key!);

                return Finish(winners);
            }
            else
            {
                if (Turn.IsStuckInThisTurn)
                    Board.Drop(Turn.PlayerIndex);

                int nextPlayerIndex = Turn.PlayerIndex + 1;
                if (nextPlayerIndex >= Board.Players.Count)
                    nextPlayerIndex = 0;
                while (Board.Players[nextPlayerIndex].IsDropped)
                {
                    nextPlayerIndex += 1;
                    if (nextPlayerIndex >= Board.Players.Count)
                        nextPlayerIndex = 0;
                }

                if (Board.Players.Where(_ => _.IsDropped == false).Count() == 1)
                {
                    return Finish(new[] { Board.Players[nextPlayerIndex] });
                }
                else
                {
                    return StartTurn(
                        ctx: ctx,
                        board: Board,
                        turn: Turn.Next(Board, nextPlayerIndex)
                        );
                }
            }
        }

        IGameState Finish(IEnumerable<Player> winners)
        {
            var image = ctx.Render(() => Board.GetBoardGrid(null));

            var embed = new DiscordEmbedBuilder()
                .AddField(winners.Count() > 1 ? "Winners" : "Winner", string.Join(", ", winners.Select(_ => _.Disboard.Name)), inline: true);

            if (winners.Count() == 1)
                embed.Color = new DiscordColor(winners.First().Color);

            ctx.SendImage(image, "@here", embed);

            ctx.OnFinish();

            return NullState.New;
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
            var image = ctx.Render(() => Board.GetBoardGrid((Turn.PlayerIndex, Board.GetReachables(CurrentPlayer, Turn.Dices, Turn.RemainMove))));

            var rank = Rank.Calculate(Turn.Dices);
            var rerollTexts = Enumerable.Range(0, Turn.RemainReroll).Select(_ => ":arrows_counterclockwise:");
            var rerollString = string.Join(" ", rerollTexts);
            var moveTexts = Enumerable.Range(0, Turn.RemainMove).Select(_ => ":arrow_right:");
            var moveString = string.Join(" ", moveTexts);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = Turn.Dices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);

            var brushes = Board.Players.Count == 2 ? new[] { "#005AC2", "#C21000" } : new[] { "#C21200", "#C2A813", "#13C264", "#190AC2" };
            var embed = new DiscordEmbedBuilder()
                    .WithColor(new DiscordColor(brushes[Turn.PlayerIndex]))
                    .AddField($"{rank.Initial} `{rank.Name}`", diceString, inline: true);
            if (Turn.RemainReroll > 0)
                embed.AddField("Reroll", rerollString, inline: true);
            if (Turn.IsStuckInThisTurn)
                embed.AddField("Stuck!", "이대로 턴 종료시 패배합니다.", inline: true);
            else
                embed.AddField("Move", moveString, inline: true);
            ctx.SendImage(image, $"{CurrentPlayer.Disboard.Mention} {CurrentPlayer.Disboard.Name}'s turn", embed);
        }
    }
}
