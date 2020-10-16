using Disboard;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Vechu
{
    class BoardContext
    {
        public IReadOnlyList<Player> Players { get; }
        public Dictionary<Player, int> ScoreDict { get; }

        public static BoardContext New(IReadOnlyList<Player> players)
            => new BoardContext(players);

        BoardContext(IReadOnlyList<Player> players)
        {
            Players = players;
            ScoreDict = players.Enumerate().ToDictionary(_ => _.elem, _ => 0);
        }
        public void Add(Player currentPlayer, int deltaScore)
            => ScoreDict[currentPlayer] += deltaScore;

        public Disgrid.Disgrid GetBoardGrid((Player player, int[] dices)? CurrentState)
        {
            int rowCount = 2;
            int columnCount = Players.Count;
            var grid = new Disgrid.Disgrid(rowCount, columnCount);

            grid.AddStyle<RowDefinition>(RowDefinition.MinHeightProperty, 30.0);
            grid.AddStyle<ColumnDefinition>(ColumnDefinition.MinWidthProperty, 80.0);

            // 그리드 전역에 스타일을 추가할 수 있습니다.
            grid.AddStyle<Label>(Label.FontSizeProperty, 12.0);

            foreach (var (playerIndex, player) in Players.Enumerate())
            {
                int score = ScoreDict[player];
                grid.Add(0, playerIndex, player.Name);

                string scoreText;
                if (CurrentState.HasValue && player == CurrentState.Value.player)
                {
                    int deltaScore = CurrentState.Value.dices[0] * CurrentState.Value.dices[1];
                    scoreText = $"{score - deltaScore} 🡐 {score} 🡒 {score + deltaScore}";
                }
                else
                {
                    scoreText = $"{score}";
                }
                // 레이블 각각에도 스타일을 추가할 수 있습니다.
                grid.Add(1, playerIndex, scoreText).FontWeight = FontWeights.Bold;
            }

            return grid;
        }
    }
}
