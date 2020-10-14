using System.Collections.Generic;
using System.Linq;

namespace Yacht
{
    class ScoreBoard : IScoreBoard
    {
        public ScoreBoard()
        {
            var upper = new List<IScorePlace>
            {
                new BasicScorePlace("1s", "Ones", "Sum of the dice", dices => dices.Where(_ => _ == 1).Sum()),
                new BasicScorePlace("2s", "Twos", "Sum of the dice", dices => dices.Where(_ => _ == 2).Sum()),
                new BasicScorePlace("3s", "Threes", "Sum of the dice", dices => dices.Where(_ => _ == 3).Sum()),
                new BasicScorePlace("4s", "Fours", "Sum of the dice", dices => dices.Where(_ => _ == 4).Sum()),
                new BasicScorePlace("5s", "Fives", "Sum of the dice", dices => dices.Where(_ => _ == 5).Sum()),
                new BasicScorePlace("6s", "Sixes", "Sum of the dice", dices => dices.Where(_ => _ == 6).Sum()),
            };

            var bonus = new BonusScorePlace(upper);

            var smallStraights = new List<SortedSet<int>> { new SortedSet<int> { 1, 2, 3, 4 }, new SortedSet<int> { 2, 3, 4, 5 }, new SortedSet<int> { 3, 4, 5, 6 } };
            var largeStraights = new List<SortedSet<int>> { new SortedSet<int> { 1, 2, 3, 4, 5 }, new SortedSet<int> { 2, 3, 4, 5, 6 } };

            var lower = new List<IScorePlace>
            {
                new BasicScorePlace("ch", "Choice", "Sum of the dice", dices => dices.Sum()),
                new BasicScorePlace("4k", "Four of a kind", "Sum of the dice", dices => dices.GroupBy(_=>_).Any(group => group.Count() >= 4) ? dices.Sum() : 0),
                new BasicScorePlace("fh", "Full House", "Sum of the dice", dices => dices.GroupBy(_=>_).Any(group => group.Count() == 2) && dices.GroupBy(_=>_).Any(group => group.Count() == 3) ? dices.Sum() : 0),
                new BasicScorePlace("ss", "Small straight", "15 points", dices => smallStraights.Any(_ => _.IsSubsetOf(dices)) ? 15 : 0),
                new BasicScorePlace("ls", "Large straight", "30 points", dices => largeStraights.Any(_ => _.IsSubsetOf(dices)) ? 30 : 0),
                new BasicScorePlace("yt", "Yacht", "50 points", dices => dices.GroupBy(_=>_).Any(group => group.Count() == 5) ? 50 : 0),
            };

            var places = upper.Append(bonus).Concat(lower);
            Places = places.ToDictionary(_ => _.Initial);
        }

        public IReadOnlyDictionary<string, IScorePlace> Places { get; }

        public void Submit(string initial, int[] dices)
        {
            IScorePlace? place = Places.GetValueOrDefault(initial.ToLower());
            if (place == null)
            {
                throw new CommandNotFoundException();
            }
            else if (place.IsOpen == false)
            {
                throw new InvalidOperationException();
            }
            else
            {
                place.Submit(dices);
            }
        }

        public int TotalScore => Places.Values.Sum(_ => _.CurrentScore);
    }
}
