using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Xanth
{
    partial class TurnState
    {
        Dictionary<Slot, Slot.Permission> GetReachables()
        {
            var marker = Board.MarkerDict[CurrentPlayer];
            var permissions = Board.Board.Slots.SelectMany((slots, row) => slots.Select((slot, column) => ((row, column), slot.GetPermission(CurrentPlayer, Turn.Dices))))
                .ToDictionary(_ => _.Item1, _ => _.Item2);

            // 겹칠 수 없음
            foreach (var slot in Board.Players.Where(_ => _ != CurrentPlayer).Select(_ => Board.MarkerDict[_]).Select(_ => (_.Row, _.Column)))
                permissions[slot] = Slot.Permission.Unreachable;

            var currentPositions = new[] { (marker.Row, marker.Column) }.ToImmutableSortedSet();
            var reachables = new SortedSet<(int, int)> { };
            var overwritables = new SortedSet<(int, int)> { };

            foreach (var _ in Enumerable.Range(0, Turn.RemainMove))
            {
                currentPositions = currentPositions.SelectMany(_ =>
                {
                    var nextPositions = new SortedSet<(int, int)> { };
                    foreach (var slot in new[] { (_.Row - 1, _.Column), (_.Row + 1, _.Column), (_.Row, _.Column - 1), (_.Row, _.Column + 1) })
                    {
                        var permission = permissions.GetValueOrDefault(slot, Slot.Permission.Unreachable);
                        if (permission == Slot.Permission.Reachable)
                            reachables.Add(slot);
                        if (permission == Slot.Permission.Overwritable)
                            overwritables.Add(slot);
                        if (permission >= Slot.Permission.Reachable)
                            nextPositions.Add(slot);
                    }
                    return nextPositions;
                }).ToImmutableSortedSet();
            }

            if (Board.NotMovedYet[CurrentPlayer])
                overwritables.Add((marker.Row, marker.Column));

            var result = reachables.ToDictionary(_ => Board.Board.Slots[_.Item1][_.Item2], _ => Slot.Permission.Reachable);
            overwritables.ToList().ForEach(_ => result[Board.Board.Slots[_.Item1][_.Item2]] = Slot.Permission.Overwritable);
            return result;
        }
    }
}
