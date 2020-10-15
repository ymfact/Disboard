using Disboard;
using System.Collections.Generic;
using System.Linq;

namespace Xanth
{
    class Slot
    {
        public IEnumerable<int> Banneds { get; }
        public Rank Rank { get; private set; } = Rank.Empty;
        public Player? Owner { get; private set; } = null;
        public Slot(int bannedOnRow, int bannedOnColumn)
            => Banneds = new[] { bannedOnRow, bannedOnColumn };

        public bool IsWritable(int[] dices)
            => Rank.Calculate(dices).Order >= Rank.Order && dices.All(_ => !Banneds.Contains(_));

        public void Write(Player owner, Rank rank)
        {
            Owner = owner;
            Rank = rank;
        }
    }
}
