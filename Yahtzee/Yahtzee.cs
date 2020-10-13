using Disboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Disboard.Macro;

namespace Yahtzee
{
    class Yahtzee : IGame
    {
        private const int NAME_LEN = 16;
        private readonly Random random = new Random();
        private readonly IReadOnlyList<Player> _players;
        private readonly Action _onFinish;
        private readonly IReadOnlyDictionary<Player, IScoreBoard> _scoreBoards;
        private int _currentPlayerIndex = 0;
        private int[] _currentDices = { 0, 0, 0, 0, 0 };
        private int _currentRemainReroll = 0;

        private int[] CurrentDices
        {
            get => _currentDices;
            set
            {
                Debug.Assert(value.Length == 5);
                _currentDices = value;
            }
        }

        private SendType Send { get; }

        private Player? CurrentPlayer => _currentPlayerIndex != -1 ? _players[_currentPlayerIndex] : null;

        public Yahtzee(GameInitializeData initializer)
        {
            Send = initializer.Send;
            _players = initializer.Players.OrderBy(_ => random.Next()).ToList();
            _onFinish = initializer.OnFinish;
            _scoreBoards = _players.ToDictionary(_ => _, _ => new ScoreBoard() as IScoreBoard);
        }

        public async Task Start()
        {
            await Send("`명령어: R 23456, S 4k`");
            await StartTurn();
        }

        public Task OnGroup(Player player, string message)
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
            return Task.CompletedTask;
        }
        private Task Reroll(string message)
        {
            if (_currentRemainReroll <= 0)
            {
                return Send(W("남은 리롤 기회가 없습니다. 점수를 적을 항목을 선택하세요. 예시: S 3k"));
            }
            var split = message.Split();
            if (split.Length != 2)
            {
                return Send(W("리롤할 주사위를 입력하세요. 예시: R 334"));
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
                newDices.AddRange(Enumerable.Range(0, 5 - newDices.Count).Select(_ => random.Next(6) + 1));
                CurrentDices = newDices.ToArray();
                _currentRemainReroll -= 1;
                return PrintTurn();
            }
            catch (System.FormatException)
            {
                return Send(W("리롤할 주사위를 다시 입력하세요. 예시: R 334"));
            }
        }
        private Task Submit(string message)
        {
            Debug.Assert(CurrentPlayer != null);

            var split = message.Split();
            var scoreBoard = _scoreBoards[CurrentPlayer];
            if (split.Length != 2)
            {
                return Send(W("이니셜을 입력하세요.예시: S 3k"));
            }
            var initial = split[1];
            try
            {
                scoreBoard.Submit(initial, CurrentDices);
                return ProceedAndStartTurn();
            }
            catch (System.InvalidOperationException)
            {
                return Send(W("이미 점수를 채운 항목입니다."));
            }
            catch (CommandNotFoundException)
            {
                return Send(W("올바른 이니셜을 입력하세요. 예시: S 3k"));
            }
        }
        private async Task ProceedAndStartTurn()
        {
            if (_scoreBoards.Values.All(_ => _.Places.Values.All(_ => _.IsOpen == false)))
            {
                _currentPlayerIndex = -1;
                await PrintBoard();
                var highestScore = _scoreBoards.Values.Select(_ => _.TotalScore).OrderByDescending(_ => _).First();
                var winners = _players.Where(_ => _scoreBoards[_].TotalScore == highestScore).Select(_ => _.Name);
                var winnerString = winners.Count() > 1 ? "Winners: " : "Winner: ";
                winnerString += string.Join(", ", winners);
                await Send(W(winnerString));
                _onFinish();
            }
            else
            {
                _currentPlayerIndex += 1;
                if (_currentPlayerIndex >= _players.Count)
                {
                    _currentPlayerIndex = 0;
                }
                await StartTurn();
            }
        }
        private Task StartTurn()
        {
            CurrentDices = Enumerable.Range(0, 5).Select(_ => random.Next(6) + 1).ToArray();
            _currentRemainReroll = 2;
            return PrintTurn();
        }
        private async Task PrintTurn()
        {
            Debug.Assert(CurrentPlayer != null);

            await PrintBoard();

            var checkTexts = Enumerable.Range(0, 3).Reverse().Select(_ => _ < _currentRemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var checkString = string.Join(" ", checkTexts);
            var turnIndicator = $"{CurrentPlayer.Mention} {CurrentPlayer.Name}'s turn, Reroll: " + checkString;
            await Send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = CurrentDices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            await Send(diceString);
        }
        private Task PrintBoard()
        {
            var printString = $"{' ',2} {' ',15} {' ',17}";
            foreach (Player player in _players)
            {
                printString += TrimName(player.Name);
            }
            var scoreBoard = _scoreBoards.First().Value.Places.Values; // arbitrary one
            foreach (IScorePlace place in scoreBoard)
            {
                var initial = place.Initial;
                printString += $"\n{initial,2} {place.Name,15} {place.Desc,17}";
                foreach (Player player in _players)
                {
                    var placeOfUser = _scoreBoards[player].Places[initial];
                    var currentScoreString = placeOfUser.CurrentScoreString;
                    var estimateScore = placeOfUser.CalculateScore(CurrentDices);
                    bool isShowEstimation = player == CurrentPlayer && placeOfUser.IsOpen;
                    var scoreText = isShowEstimation ? $"({estimateScore})" : $"{currentScoreString}";
                    printString += $"{scoreText,NAME_LEN}";
                }
            }
            printString += $"\n{' ',2} {' ',15} {"TOTAL",17}";
            foreach (Player player in _players)
            {
                printString += $"{_scoreBoards[player].TotalScore,NAME_LEN}";
            }

            return Send(W(printString));
        }

        private string TrimName(string name)
        {
            int width = 0;
            int wideCharCount = 0;
            string acc = "";
            foreach (char c in name)
            {
                if (c < 128)
                {
                    if (width + 1 > NAME_LEN)
                        break;
                    width += 1;
                }
                else
                {
                    if (width + 2 > NAME_LEN)
                        break;
                    wideCharCount += 1;
                    width += 2;
                }
                acc += c;
            }
            return acc.PadLeft(NAME_LEN - wideCharCount);
        }
    }
}
