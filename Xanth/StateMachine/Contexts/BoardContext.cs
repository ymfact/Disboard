using Disboard;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xanth
{
    class BoardContext
    {
        public IReadOnlyList<Player> Players { get; }
        public IReadOnlyList<int> BannedOnRows { get; }
        public IReadOnlyList<int> BannedOnColumns { get; }
        public Board Board { get; }
        public Dictionary<Player, Marker> MarkerDict { get; }
        public Dictionary<Player, bool> NotMovedYet { get; }

        public static BoardContext New(IReadOnlyList<Player> players)
            => new BoardContext(players);

        BoardContext(IReadOnlyList<Player> players)
        {
            Players = players;

            var markers = new[] { new Marker(3, 0), new Marker(0, 3) };
            MarkerDict = players.Enumerate().ToDictionary(_ => _.elem, _ => markers[_.index]);
            NotMovedYet = players.ToDictionary(_ => _, _ => true);

            var random = new Random();
            Func<int> rollDice = () => random.Next(1, 7);
            BannedOnRows = Enumerable.Range(0, 4).Select(_ => rollDice()).ToList();
            BannedOnColumns = Enumerable.Range(0, 4).Select(_ => rollDice()).ToList();

            Board = new Board(BannedOnRows, BannedOnColumns);
        }
        public void Submit(Player currentPlayer, string initials, int[] dices)
        {
            Marker marker = MarkerDict[currentPlayer];

            List<Slot> slotsToWrite = new List<Slot> { };
            int len = initials.Length;
            for (int i = 0; i < len; i++)
            {
                if (NotMovedYet[currentPlayer] && i == 0)
                {
                    if(initials[0] == '!')
                        continue;

                    var startSlot = Board.Slots[marker.Row][marker.Column];
                    slotsToWrite.Add(startSlot);
                }
                char direction = initials[i];
                bool isSkipWriting = false;
                if (i + 1 < len && initials[i + 1] == '!')
                {
                    isSkipWriting = true;
                    i += 1;
                }
                if (direction == 'w')
                    marker = marker.Move(-1, 0);
                else if (direction == 's')
                    marker = marker.Move(+1, 0);
                else if (direction == 'a')
                    marker = marker.Move(0, -1);
                else if (direction == 'd')
                    marker = marker.Move(0, +1);
                else
                    throw new InvalidOperationException();

                var slot = Board.Slots[marker.Row][marker.Column];
                bool writable = slot.IsWritable(dices);
                if (false == writable && slot.Owner != null && slot.Owner != currentPlayer)
                    throw new MoveProhibitedException();

                if (writable && false == isSkipWriting)
                    slotsToWrite.Add(slot);
            }

            var rank = Rank.Calculate(dices);
            foreach (var slot in slotsToWrite)
            {
                slot.Write(currentPlayer, rank);
            }
            MarkerDict[currentPlayer] = marker;
            NotMovedYet[currentPlayer] = false;
        }

        public Disgrid.Disgrid GetBoardGrid()
        {
            int rowCount = 1 + 4;
            int columnCount = 1 + 4;
            var grid = new Disgrid.Disgrid(rowCount, columnCount);

            grid.InnerGrid.RowDefinitions[0].MinHeight = 20.0;
            grid.InnerGrid.ColumnDefinitions[0].MinWidth = 20.0;
            grid.AddStyle<RowDefinition>(RowDefinition.MinHeightProperty, 80.0);
            grid.AddStyle<ColumnDefinition>(ColumnDefinition.MinWidthProperty, 80.0);

            grid.AddStyle<Label>(Label.FontSizeProperty, 12.0);

            var darkBrushes = new[] { "#01326B".Brush(), "#6B0900".Brush() };
            var brushes = new[] { "#005AC2".Brush(), "#C21000".Brush() };

            foreach (var (row, banned) in BannedOnRows.Enumerate())
            {
                var label = grid.Add(1 + row, 0, $"{banned}");
                label.FontWeight = FontWeights.Bold;
                label.FontSize = 18;
            }

            foreach (var (column, banned) in BannedOnColumns.Enumerate())
            {
                var label = grid.Add(0, 1 + column, $"{banned}");
                label.FontWeight = FontWeights.Bold;
                label.FontSize = 18;
            }

            foreach (var (row, slots) in Board.Slots.Enumerate())
                foreach (var (column, slot) in slots.Enumerate())
                    if (slot.Owner != null)
                    {
                        string text = $"{slot.Rank.Initial}\n";
                        int playerIndex = Players.FindIndex(_ => _ == slot.Owner)!.Value;
                        var brush = darkBrushes[playerIndex];
                        var label = grid.Add(1 + row, 1 + column, text);
                        label.Background = brush;
                        label.FontWeight = FontWeights.Bold;
                        label.FontSize = 14;
                    }

            foreach (var (playerIndex, (player, marker)) in Players.Zip(Players.Select(_ => MarkerDict[_])).Enumerate())
            {
                var brush = brushes[playerIndex];
                string text = $"\n{player.Name}";
                var label = grid.Add(1 + marker.Row, 1 + marker.Column, text);
                label.Foreground = brush;
            }

            return grid;
        }
    }
}
