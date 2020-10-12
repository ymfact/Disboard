using Disboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Disboard.GameInitializer;
using static Disboard.Macro;

namespace Yahtzee
{
    class Yahtzee : IGame
    {
        private const int NAME_LEN = 16;
        private readonly Random random = new Random();
        private readonly SendType _send;
        private readonly IReadOnlyList<User> _users;
        private readonly Action _onFinish;
        private readonly IReadOnlyDictionary<User.IdType, IScoreBoard> _scoreBoards;
        private int _currentUserIndex = 0;
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

        private User? CurrentUser => _currentUserIndex != -1 ? _users[_currentUserIndex] : null;

        public Yahtzee(GameInitializer initializer)
        {
            _send = initializer.Send;
            _users = initializer.Users.OrderBy(_ => random.Next()).ToList();
            _onFinish = initializer.OnFinish;
            _scoreBoards = _users.ToDictionary(_ => _.Id, _ => new ScoreBoard() as IScoreBoard);
        }

        public async Task Start()
        {
            await _send("`명령어: R 23456, S 4k`");
            await StartTurn();
        }

        public bool AcceptsOnGroup(User.IdType authorId, string message)
        {
            if (authorId == CurrentUser?.Id)
            {
                var split = message.Split();
                if (split.Length > 0 && split[0].ToLower() == "r")
                {
                    return true;
                }
                if (split.Length > 0 && split[0].ToLower() == "s")
                {
                    return true;
                }
            }
            return false;
        }

        public async Task OnGroup(User.IdType authorId, string message)
        {
            if (authorId == CurrentUser?.Id)
            {
                var split = message.Split();
                if (split.Length > 0 && split[0].ToLower() == "r")
                {
                    await Reroll(message);
                }
                if (split.Length > 0 && split[0].ToLower() == "s")
                {
                    await Submit(message);
                }
            }
        }
        private async Task Reroll(string message)
        {
            if (_currentRemainReroll <= 0)
            {
                await _send(W("남은 리롤 기회가 없습니다. 점수를 적을 항목을 선택하세요. 예시: S 3k"));
                return;
            }
            var split = message.Split();
            if (split.Length != 2)
            {
                await _send(W("리롤할 주사위를 입력하세요. 예시: R 334"));
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
                newDices.AddRange(Enumerable.Range(0, 5 - newDices.Count).Select(_ => random.Next(6) + 1));
                CurrentDices = newDices.ToArray();
                _currentRemainReroll -= 1;
                await PrintTurn();
            }
            catch (System.FormatException)
            {
                await _send(W("리롤할 주사위를 다시 입력하세요. 예시: R 334"));
                return;
            }
        }
        private async Task Submit(string message)
        {
            Debug.Assert(CurrentUser != null);

            var split = message.Split();
            var scoreBoard = _scoreBoards[CurrentUser.Id];
            if (split.Length != 2)
            {
                await _send(W("이니셜을 입력하세요.예시: S 3k"));
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
                await _send(W("이미 점수를 채운 항목입니다."));
                return;
            }
            catch (CommandNotFoundException)
            {
                await _send(W("올바른 이니셜을 입력하세요. 예시: S 3k"));
                return;
            }
        }
        private async Task ProceedAndStartTurn()
        {
            if (_scoreBoards.Values.All(_ => _.Places.Values.All(_ => _.IsOpen == false)))
            {
                _currentUserIndex = -1;
                await PrintBoard();
                var highestScore = _scoreBoards.Values.Select(_ => _.TotalScore).OrderByDescending(_ => _).First();
                var winners = _users.Where(_ => _scoreBoards[_.Id].TotalScore == highestScore).Select(_ => _.Name);
                var winnerString = winners.Count() > 1 ? "Winners: " : "Winner: ";
                winnerString += string.Join(", ", winners);
                await _send(W(winnerString));
                _onFinish();
            }
            else
            {
                _currentUserIndex += 1;
                if (_currentUserIndex >= _users.Count)
                {
                    _currentUserIndex = 0;
                }
                await StartTurn();
            }
        }
        private async Task StartTurn()
        {
            CurrentDices = Enumerable.Range(0, 5).Select(_ => random.Next(6) + 1).ToArray();
            _currentRemainReroll = 2;
            await PrintTurn();
        }
        private async Task PrintTurn()
        {
            Debug.Assert(CurrentUser != null);

            await PrintBoard();

            var checkTexts = Enumerable.Range(0, 3).Reverse().Select(_ => _ < _currentRemainReroll).Select(_ => _ ? ":arrows_counterclockwise:" : ":ballot_box_with_check:");
            var checkString = string.Join(" ", checkTexts);
            var turnIndicator = $"{CurrentUser.Mention} {CurrentUser.Name}'s turn, Reroll: " + checkString;
            await _send(turnIndicator);

            var diceTextTemplates = new List<string> { ":zero:", ":one:", ":two:", ":three:", ":four:", ":five:", ":six:" };
            var diceTexts = CurrentDices.Select(_ => diceTextTemplates[_]);
            var diceString = string.Join(" ", diceTexts);
            await _send(diceString);
        }
        private async Task PrintBoard()
        {
            var printString = $"{' ',2} {' ',15} {' ',17}";
            foreach (User user in _users)
            {
                printString += TrimName(user.Name);
            }
            var scoreBoard = _scoreBoards.First().Value.Places.Values; // arbitrary one
            foreach (IScorePlace place in scoreBoard)
            {
                var initial = place.Initial;
                printString += $"\n{initial,2} {place.Name,15} {place.Desc,17}";
                foreach (User user in _users)
                {
                    var placeOfUser = _scoreBoards[user.Id].Places[initial];
                    var currentScoreString = placeOfUser.CurrentScoreString;
                    var estimateScore = placeOfUser.CalculateScore(CurrentDices);
                    bool isShowEstimation = user == CurrentUser && placeOfUser.IsOpen;
                    var scoreText = isShowEstimation ? $"({estimateScore})" : $"{currentScoreString}";
                    printString += $"{scoreText,NAME_LEN}";
                }
            }
            printString += $"\n{' ',2} {' ',15} {"TOTAL",17}";
            foreach (User user in _users)
            {
                printString += $"{_scoreBoards[user.Id].TotalScore,NAME_LEN}";
            }

            await _send(W(printString));
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
