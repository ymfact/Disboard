using Disboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Disboard.Macro;

namespace Yacht
{
    class Yacht : Game
    {
        int _currentPlayerIndex = 0;
        int[] __currentDices = { };
        int _currentRemainReroll = 0;

        Random Random { get; } = new Random();
        IReadOnlyDictionary<Player, IScoreBoard> ScoreBoards { get; }

        int[] CurrentDices
        {
            get => __currentDices;
            set
            {
                Debug.Assert(value.Length == 5);
                __currentDices = value;
            }
        }

        new IReadOnlyList<Player> Players => ScoreBoards.Keys.ToList();
        Player? CurrentPlayer => _currentPlayerIndex != -1 ? Players[_currentPlayerIndex] : null;

        public Yacht(GameInitializeData initData) : base(initData)
        {
            var players = base.Players.OrderBy(_ => Random.Next()).ToList();
            ScoreBoards = players.ToDictionary(_ => _, _ => new ScoreBoard() as IScoreBoard);
        }

        public override async Task Start()
        {
            await Send("`명령어: R 23456, S 4k`");
            await StartTurn();
        }

        public override async Task OnGroup(Player player, string message)
        {
            if (player == CurrentPlayer)
            {
                var split = message.Split();
                if (split.Length > 0 && split[0].ToLower() == "r")
                {
                    await Reroll(message);
                }
                else if (split.Length > 0 && split[0].ToLower() == "s")
                {
                    await Submit(message);
                }
            }
        }
        async Task Reroll(string message)
        {
            if (_currentRemainReroll <= 0)
            {
                await Send(W("남은 리롤 기회가 없습니다. 점수를 적을 항목을 선택하세요. 예시: S 3k"));
                return;
            }
            var split = message.Split();
            if (split.Length != 2)
            {
                await Send(W("리롤할 주사위를 입력하세요. 예시: R 334"));
                return;
            }
            try
            {
                var dicesToReroll = split[1].Select(_ => int.Parse(_.ToString()));
                var newDices = CurrentDices.ToList(); //copy
                foreach (int diceToReroll in dicesToReroll)
                {
                    if (newDices.Contains(diceToReroll))
                    {
                        newDices.RemoveAt(newDices.LastIndexOf(diceToReroll));
                    }
                    else
                    {
                        throw new System.FormatException();
                    }
                }
                newDices.AddRange(Enumerable.Range(0, 5 - newDices.Count).Select(_ => Random.Next(6) + 1));
                CurrentDices = newDices.ToArray();
                _currentRemainReroll -= 1;
                await PrintTurn();
            }
            catch (System.FormatException)
            {
                await Send(W("리롤할 주사위를 다시 입력하세요. 예시: R 334"));
            }
        }

        async Task Submit(string message)
        {
            Debug.Assert(CurrentPlayer != null);

            var split = message.Split();
            var scoreBoard = ScoreBoards[CurrentPlayer];
            if (split.Length != 2)
            {
                await Send(W("이니셜을 입력하세요.예시: S 3k"));
                return;
            }
            var initial = split[1];
            try
            {
                scoreBoard.Submit(initial, CurrentDices);
                await ProceedAndStartTurn();
            }
            catch (System.InvalidOperationException)
            {
                await Send(W("이미 점수를 채운 항목입니다."));
            }
            catch (CommandNotFoundException)
            {
                await Send(W("올바른 이니셜을 입력하세요. 예시: S 3k"));
            }
        }

        async Task ProceedAndStartTurn()
        {
            if (ScoreBoards.Values.All(_ => _.Places.Values.All(_ => _.IsOpen == false)))
            {
                _currentPlayerIndex = -1;
                await SendImage(GetBoardImage());
                var highestScore = ScoreBoards.Values.Select(_ => _.TotalScore).OrderByDescending(_ => _).First();
                var winners = Players.Where(_ => ScoreBoards[_].TotalScore == highestScore).Select(_ => _.Name);
                var winnerString = winners.Count() > 1 ? "Winners: " : "Winner: ";
                winnerString += W(string.Join(", ", winners));
                await Send(winnerString);
                OnFinish();
            }
            else
            {
                _currentPlayerIndex += 1;
                if (_currentPlayerIndex >= Players.Count)
                {
                    _currentPlayerIndex = 0;
                }
                await StartTurn();
            }
        }

        async Task StartTurn()
        {
            CurrentDices = Enumerable.Range(0, 5).Select(_ => Random.Next(6) + 1).ToArray();
            _currentRemainReroll = 2;
            await PrintTurn();
        }

        async Task PrintTurn()
        {
            Debug.Assert(CurrentPlayer != null);

            await SendImage(GetBoardImage());

            var checkTexts = Enumerable.Range(0, 3).Reverse().Select(_ => _ < _currentRemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var checkString = string.Join(" ", checkTexts);
            var turnIndicator = $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn, Reroll: " + checkString;
            await Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = CurrentDices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            await Send(diceString);
        }

        Stream GetBoardImage() => Render(() =>
        {
            var scorePlaces = ScoreBoards.Values.First().Places.Values.ToList();

            int rowCount = 1 + scorePlaces.Count + 1;
            int columnCount = 3 + Players.Count;
            var grid = new Disgrid.Disgrid(rowCount, columnCount);

            // 그리드 전역에 스타일을 추가할 수 있습니다.
            grid.AddStyle<Label>(Label.FontSizeProperty, 18.0);

            // 레이블 각각에도 스타일을 추가할 수 있습니다.
            foreach (var (i, player) in Players.Enumerate())
                grid.Add(0, 3 + i, player.Name).FontWeight = FontWeights.Bold;

            foreach (var (i, place) in scorePlaces.Enumerate())
            {
                int row = 1 + i;
                grid.Add(row, 0, place.Initial);
                grid.Add(row, 1, place.Name);
                grid.Add(row, 2, place.Desc);
                foreach (var (j, (player, scoreBoard)) in ScoreBoards.Enumerate())
                {
                    var placeOfUser = scoreBoard.Places[place.Initial];
                    var currentScoreString = placeOfUser.CurrentScoreString;
                    var estimateScore = placeOfUser.CalculateScore(CurrentDices);
                    bool isShowEstimation = player == CurrentPlayer && placeOfUser.IsOpen;
                    var scoreText = isShowEstimation ? $"({estimateScore})" : $"{currentScoreString}";
                    grid.Add(row, 3 + j, scoreText);
                }
            }
            int totalScoreRow = 1 + scorePlaces.Count;
            grid.Add(totalScoreRow, 2, "TOTAL").FontWeight = FontWeights.Bold;

            foreach (var (i, (player, scoreBoard)) in ScoreBoards.Enumerate())
                grid.Add(totalScoreRow, 3 + i, scoreBoard.TotalScore.ToString()).FontWeight = FontWeights.Bold; ;

            return grid;
        });
    }
}
