using Disboard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Yacht
{
    class BoardContext
    {
        public IReadOnlyList<DisboardPlayer> Players { get; }
        public IReadOnlyDictionary<DisboardPlayer, IScoreBoard> ScoreBoardDict { get; }
        IEnumerable<IScoreBoard> ScoreBoards => Players.Select(_ => ScoreBoardDict[_]);

        BoardContext(
            IReadOnlyList<DisboardPlayer> players,
            IReadOnlyDictionary<DisboardPlayer, IScoreBoard> scoreBoards
            )
        {
            Players = players;
            ScoreBoardDict = scoreBoards;
        }

        public static BoardContext New(IReadOnlyList<DisboardPlayer> players)
            => new BoardContext(
                players: players,
                scoreBoards: players.ToDictionary(_ => _, _ => new ScoreBoard() as IScoreBoard)
                );

        public Disgrid.Disgrid GetBoardGrid((DisboardPlayer currentPlayer, int[] currentDices)? currentState)
        {
            var scorePlaces = ScoreBoards.First().Places.Values.ToList();

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
                foreach (var (j, (player, scoreBoard)) in Players.Zip(ScoreBoards).Enumerate())
                {
                    var placeOfUser = scoreBoard.Places[place.Initial];
                    var currentScoreString = placeOfUser.CurrentScoreString;
                    string scoreText;
                    bool isShowEstimation = currentState.HasValue && player == currentState.Value.currentPlayer && placeOfUser.IsOpen;
                    if (isShowEstimation)
                    {
                        var estimateScore = placeOfUser.CalculateScore(currentState!.Value.currentDices);
                        scoreText = $"({estimateScore})";
                    }
                    else
                    {
                        scoreText = $"{currentScoreString}";
                    }
                    grid.Add(row, 3 + j, scoreText);
                }
            }
            int totalScoreRow = 1 + scorePlaces.Count;
            grid.Add(totalScoreRow, 2, "TOTAL").FontWeight = FontWeights.Bold;

            foreach (var (i, scoreBoard) in ScoreBoards.Enumerate())
                grid.Add(totalScoreRow, 3 + i, scoreBoard.TotalScore.ToString()).FontWeight = FontWeights.Bold;

            return grid;
        }
    }
}
