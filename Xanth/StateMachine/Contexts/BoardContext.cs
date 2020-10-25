using Disboard;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xanth
{
    class BoardContext
    {
        public IReadOnlyDictionary<Disboard.DisboardPlayer, Player> PlayerDict { get; }
        public IReadOnlyList<Player> Players { get; }
        public IReadOnlyList<int> BannedOnRows { get; }
        public IReadOnlyList<int> BannedOnColumns { get; }
        public Board Board { get; }
        public int BoardSize { get; }

        public static BoardContext New(IReadOnlyList<Disboard.DisboardPlayer> players)
            => new BoardContext(players);

        BoardContext(IReadOnlyList<Disboard.DisboardPlayer> players)
        {
            IList<Marker> markers;
            if (players.Count == 2)
            {
                BoardSize = 4;
                markers = new[] { new Marker(3, 0, BoardSize), new Marker(0, 3, BoardSize) };
            }
            else
            {
                BoardSize = 5;
                markers = new[] { new Marker(4, 1, BoardSize), new Marker(3, 4, BoardSize), new Marker(0, 3, BoardSize), new Marker(0, 1, BoardSize) };
            }

            PlayerDict = players.Enumerate().ToDictionary(_ => _.elem, _ => new Player(players, _.elem, markers[_.index]));
            Players = players.Select(_ => PlayerDict[_]).ToList();

            Random random = new Random();
            int rollDice() => random.Next(1, 7);
            BannedOnRows = Enumerable.Range(0, BoardSize).Select(_ => rollDice()).ToList();
            BannedOnColumns = Enumerable.Range(0, BoardSize).Select(_ => rollDice()).ToList();

            Board = new Board(BannedOnRows, BannedOnColumns);
        }
        public void Submit(Player currentPlayer, string initials, int[] dices)
        {
            Marker marker = currentPlayer.Marker;

            List<Slot> slotsToWrite = new List<Slot> { };
            int len = initials.Length;
            for (int i = 0; i < len; i++)
            {
                if (currentPlayer.RemainFirstMoveBonus && i == 0)
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
                    throw new InvalidKeywordException();

                bool overlapped = Players.Where(_ => _ != currentPlayer).Select(_ => _.Marker).Select(_ => (_.Row, _.Column)).Any(_ => _ == (marker.Row, marker.Column));
                if (overlapped)
                    throw new MoveProhibitedException();

                var slot = Board.Slots[marker.Row][marker.Column];
                bool isReachable = slot.GetPermission(currentPlayer, dices) >= Slot.Permission.Reachable;
                if (false == isReachable)
                    throw new MoveProhibitedException();

                bool isOverwritable = slot.GetPermission(currentPlayer, dices) == Slot.Permission.Overwritable;
                if (false == isSkipWriting && isOverwritable)
                    slotsToWrite.Add(slot);

                var emptySlots = Board.Slots.SelectMany(_ => _.Where(_ => _.Owner == null)).Where(_ => slotsToWrite.Contains(_) == false);
                var isGameEnd = emptySlots.Count() == 0;
                if (isGameEnd && i != len - 1)
                    throw new MoveAfterGameEndException();
            }

            var rank = Rank.Calculate(dices);
            foreach (var slot in slotsToWrite)
            {
                slot.Write(currentPlayer, rank);
            }
            currentPlayer.Marker = marker;
            currentPlayer.RemainFirstMoveBonus = false;
        }

        public Disgrid.Disgrid GetBoardGrid((Player player, Dictionary<Slot, Slot.Permission> reachables)? CurrentState)
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
                        label.Background = slot.Owner.DarkColor.Brush();
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
                            border.BorderBrush = CurrentState.Value.player.Color.Brush();

                    }
                }

            foreach (var (playerIndex, player) in Players.Enumerate())
            {
                if (player.IsDropped)
                    continue;

                var brush = player.Color.Brush();
                string text = $"\n{player.Disboard.Name}";
                var label = grid.Add(1 + player.Marker.Row, 1 + player.Marker.Column, text);
                label.Foreground = brush;
            }

            return grid;
        }
        public Dictionary<Slot, Slot.Permission> GetReachables(Player player, int[] dices, int remainMove)
        {
            var permissions = Board.Slots.SelectMany((slots, row) => slots.Select((slot, column) => ((row, column), slot.GetPermission(player, dices))))
                .ToDictionary(_ => _.Item1, _ => _.Item2);

            // 겹칠 수 없음
            foreach (var slot in Players.Where(_ => (_ != player) && (_.IsDropped == false)).Select(_ => _.Marker).Select(_ => (_.Row, _.Column)))
                permissions[slot] = Slot.Permission.Unreachable;

            var currentPositions = new[] { (player.Marker.Row, player.Marker.Column) }.ToImmutableSortedSet();
            var reachables = new SortedSet<(int, int)> { };
            var overwritables = new SortedSet<(int, int)> { };

            foreach (var _ in Enumerable.Range(0, remainMove))
            {
                currentPositions = currentPositions.SelectMany(_ =>
                {
                    var nextPositions = new SortedSet<(int, int)> { };
                    foreach (var slot in new[] { (_.Row - 1, _.Column), (_.Row + 1, _.Column), (_.Row, _.Column - 1), (_.Row, _.Column + 1) })
                    {
                        var permission = permissions.GetValueOrDefault(slot, Slot.Permission.Unreachable);
                        if (permission >= Slot.Permission.Reachable)
                            reachables.Add(slot);
                        if (permission == Slot.Permission.Overwritable)
                            overwritables.Add(slot);
                        if (permission >= Slot.Permission.Reachable)
                            nextPositions.Add(slot);
                    }
                    return nextPositions;
                }).ToImmutableSortedSet();
            }

            // 이동이 처음이면 자기 자리도 칠할 수 있지만, 갇힌 상태라면 아님
            if (player.RemainFirstMoveBonus && reachables.Count > 0)
                overwritables.Add((player.Marker.Row, player.Marker.Column));

            var result = reachables.ToDictionary(_ => Board.Slots[_.Item1][_.Item2], _ => Slot.Permission.Reachable);
            overwritables.ToList().ForEach(_ => result[Board.Slots[_.Item1][_.Item2]] = Slot.Permission.Overwritable);
            return result;
        }
        public bool IsStuck(Player player, int[] dices)
        {
            var reachables = GetReachables(player, dices, 1);
            return reachables.Count == 0;
        }
        public void Drop(Player player)
            => player.IsDropped = true;
    }
}
