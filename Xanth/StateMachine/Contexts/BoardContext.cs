using Disboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        public int BoardSize { get; }

        public static BoardContext New(IReadOnlyList<Player> players)
            => new BoardContext(players);

        BoardContext(IReadOnlyList<Player> players)
        {
            Players = players;

            IList<Marker> markers;
            if (players.Count == 2)
            {
                BoardSize = 4;
                markers = new[] { new Marker(3, 0, BoardSize), new Marker(0, 3, BoardSize) };
            }
            else
            {
                BoardSize = 6;
                markers = new[] { new Marker(0, 5, BoardSize), new Marker(0, 0, BoardSize), new Marker(5, 0, BoardSize), new Marker(5, 5, BoardSize) };
            }

            MarkerDict = players.Enumerate().ToDictionary(_ => _.elem, _ => markers[_.index]);
            NotMovedYet = players.ToDictionary(_ => _, _ => true);

            Random random = new Random();
            int rollDice() => random.Next(1, 7);
            BannedOnRows = Enumerable.Range(0, BoardSize).Select(_ => rollDice()).ToList();
            BannedOnColumns = Enumerable.Range(0, BoardSize).Select(_ => rollDice()).ToList();

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
                    if (initials[0] == '!')
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
                bool isReachable = slot.GetPermission(currentPlayer, dices) >= Slot.Permission.Reachable;
                if (false == isReachable)
                    throw new MoveProhibitedException();

                bool isOverwritable = slot.GetPermission(currentPlayer, dices) == Slot.Permission.Overwritable;
                if (false == isSkipWriting && isOverwritable)
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

        public Disgrid.Disgrid GetBoardGrid((int playerIndex, Dictionary<Slot, Slot.Permission> reachables)? CurrentState)
        {
            int rowCount = 1 + BoardSize;
            int columnCount = 1 + BoardSize;
            var grid = new Disgrid.Disgrid(rowCount, columnCount);

            grid.InnerGrid.RowDefinitions[0].MinHeight = 30.0;
            grid.InnerGrid.ColumnDefinitions[0].MinWidth = 30.0;
            grid.AddStyle<RowDefinition>(RowDefinition.MinHeightProperty, 80.0);
            grid.AddStyle<ColumnDefinition>(ColumnDefinition.MinWidthProperty, 80.0);

            // 그리드 전역에 스타일을 추가할 수 있습니다.
            grid.AddStyle<Label>(Label.FontSizeProperty, 12.0);

            var darkBrushes = new[] { "#01326B".Brush(), "#6B0900".Brush() };
            var brushes = new[] { "#005AC2".Brush(), "#C21000".Brush() };
            var transparentWhite = "#99AAAAAA".Brush();

            foreach (var (row, banned) in BannedOnRows.Enumerate())
            {
                // 레이블 각각에도 스타일을 추가할 수 있습니다.
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
                {
                    if (slot.Owner != null)
                    {
                        string text = $"{slot.Rank.Initial}\n";
                        var label = grid.Add(1 + row, 1 + column, text);
                        int ownerIndex = Players.FindIndex(_ => _ == slot.Owner)!.Value;
                        label.FontWeight = FontWeights.Bold;
                        label.FontSize = 14;
                        label.Background = darkBrushes[ownerIndex];
                    }

                    if (CurrentState.HasValue)
                    {
                        var border = grid.Add(1 + row, 1 + column, "");
                        var reachability = CurrentState.Value.reachables.GetValueOrDefault(slot, Slot.Permission.Unreachable);
                        if (reachability >= Slot.Permission.Reachable)
                            border.BorderThickness = new Thickness(4);
                        if (reachability == Slot.Permission.Reachable)
                            border.BorderBrush = transparentWhite;
                        if (reachability == Slot.Permission.Overwritable)
                            border.BorderBrush = brushes[CurrentState.Value.playerIndex];

                    }
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
