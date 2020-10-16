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
        public void Write(Player owner, Rank rank)
        {
            Owner = owner;
            Rank = rank;
        }

        public enum Permission
        {
            Unreachable, Reachable, Overwritable
        }
        public Permission GetPermission(Player player, int[] dices)
        {
            var newRank = Rank.Calculate(dices);
            bool banned = dices.Any(_ => Banneds.Contains(_));

            if (Owner == null)
            {
                if (banned)
                    return Permission.Reachable;
                else
                    return Permission.Overwritable;
            }
            else if (Owner == player)
            {
                if (false == banned && newRank.Order > Rank.Order)
                    return Permission.Overwritable;
                else
                    return Permission.Reachable;
            }
            else
            {
                if (false == banned && newRank.Order >= Rank.Order)
                    return Permission.Overwritable;
                else
                    return Permission.Unreachable;
            }
        }
    }
}
