using Disboard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yacht
{
    partial class ScoreBoardGrid : UserControl
    {
        void InitGrid(int rowCount, int columnCount)
        {
            foreach (var _ in Enumerable.Range(0, rowCount))
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            foreach (var _ in Enumerable.Range(0, columnCount))
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        }
        void AddLabel(int row, int column, string text)
        {
            var label = new Label
            {
                Content = text,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.White,
            };
            Grid.SetColumn(label, column);
            Grid.SetRow(label, row);
            grid.Children.Add(label);
        }
        internal ScoreBoardGrid(IReadOnlyDictionary<Player, IScoreBoard> scoreBoardDict, Player? currentPlayer, int[] currentDices)
        {

            var players = scoreBoardDict.Keys.ToList();
            var scoreBoards = scoreBoardDict.Values.ToList();
            var scorePlaces = scoreBoards.First().Places.Values.ToList(); // arbitrary one

            InitializeComponent();
            InitGrid(1 + scorePlaces.Count + 1, 3 + scoreBoards.Count);

            foreach (var (i, player) in players.Enumerate())
                AddLabel(0, 3 + i, player.Name);

            foreach (var (i, place) in scorePlaces.Enumerate())
            {
                int row = 1 + i;
                AddLabel(row, 0, place.Initial);
                AddLabel(row, 1, place.Name);
                AddLabel(row, 2, place.Desc);
                foreach (var (j, (player, scoreBoard)) in players.Zip(scoreBoards).Enumerate())
                {
                    var placeOfUser = scoreBoard.Places[place.Initial];
                    var currentScoreString = placeOfUser.CurrentScoreString;
                    var estimateScore = placeOfUser.CalculateScore(currentDices);
                    bool isShowEstimation = player == currentPlayer && placeOfUser.IsOpen;
                    var scoreText = isShowEstimation ? $"({estimateScore})" : $"{currentScoreString}";
                    AddLabel(row, 3 + j, scoreText);
                }
            }
            int totalScoreRow = 1 + scorePlaces.Count;
            AddLabel(totalScoreRow, 2, "TOTAL");

            foreach (var (i, scoreBoard) in scoreBoards.Enumerate())
                AddLabel(totalScoreRow, 3 + i, scoreBoard.TotalScore.ToString());
        }
    }
}
