using System.Collections.Generic;
using System.Linq;

namespace Xanth
{
    class Board
    {
        public readonly IReadOnlyList<IReadOnlyList<Slot>> Slots;
        public Board(IEnumerable<int> bannedOnRows, IEnumerable<int> bannedOnColumns)
        {
            Slots = bannedOnRows.Select(bannedOnRow =>
                bannedOnColumns.Select(bannedOnColumn =>
                    new Slot(bannedOnRow, bannedOnColumn)
                ).ToList()
            ).ToList();
        }
    }
}
