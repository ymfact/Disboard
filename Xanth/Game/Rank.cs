using System;
using System.Collections.Generic;
using System.Linq;

namespace Xanth
{
    class Rank
    {
        Rank(string name, string initial, int order)
        {
            Name = name;
            Initial = initial;
            Order = order;
        }

        public string Name { get; }
        public string Initial { get; }
        public int Order { get; }

        public static Rank Empty = new Rank("Empty", "", 0);
        public static Rank NoPair = new Rank("No Pair", "O", 1);
        public static Rank OnePair = new Rank("One Pair", "P", 2);
        public static Rank ThreeStraight = new Rank("3 Straight", "3", 3);
        public static Rank Triple = new Rank("Triple", "T", 4);
        public static Rank TwoPair = new Rank("Two Pair", "2P", 5);
        public static Rank FourStraight = new Rank("4 Straight", "4", 6);
        public static Rank Xanth = new Rank("Xanth", "X", 6);

        static IList<ISet<int>> ThreeStraights { get; } = new[] { new SortedSet<int> { 1, 2, 3 }, new SortedSet<int> { 2, 3, 4 }, new SortedSet<int> { 3, 4, 5 }, new SortedSet<int> { 4, 5, 6 } };
        static IList<ISet<int>> FourStraights { get; } = new[] { new SortedSet<int> { 1, 2, 3, 4 }, new SortedSet<int> { 2, 3, 4, 5 }, new SortedSet<int> { 3, 4, 5, 6 } };
        
        public static Rank Calculate(int[] dices)
        {
            if (dices.GroupBy(_ => _).Any(_ => _.Count() == 4))
                return Xanth;
            if (FourStraights.Any(_ => _.IsSubsetOf(dices)))
                return FourStraight;
            if (dices.GroupBy(_ => _).Where(_ => _.Count() == 2).Count() == 2)
                return TwoPair;
            if (dices.GroupBy(_ => _).Any(_ => _.Count() == 3))
                return Triple;
            if (ThreeStraights.Any(_ => _.IsSubsetOf(dices)))
                return ThreeStraight;
            if (dices.GroupBy(_ => _).Any(_ => _.Count() == 2))
                return OnePair;
            return NoPair;
        }
    }
}
