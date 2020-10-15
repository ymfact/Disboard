using System;
using System.Diagnostics;

namespace Yacht
{
    class BasicScorePlace : IScorePlace
    {
        int[] __dices = { };

        int[] Dices
        {
            get => __dices;
            set
            {
                Debug.Assert(value.Length == 5);
                __dices = value;
            }
        }
        Func<int[], int> ScoreCalculator { get; }

        public BasicScorePlace(string initial, string name, string desc, Func<int[], int> scoreCalculator)
        {
            Initial = initial;
            Name = name;
            Desc = desc;
            ScoreCalculator = scoreCalculator;
        }

        public string Initial { get; }

        public string Name { get; }

        public string Desc { get; }

        public bool IsOpen
            => Dices.Length == 0;

        public virtual int CurrentScore
        {
            get
            {
                if (Dices.Length == 0)
                {
                    return 0;
                }
                else
                {
                    return CalculateScore(Dices);
                }
            }
        }

        public string CurrentScoreString
        {
            get
            {
                if (IsOpen)
                {
                    return "_";
                }
                else
                {
                    return CurrentScore.ToString();
                }
            }
        }

        public int CalculateScore(int[] dices)
            => ScoreCalculator(dices);

        public void Submit(int[] dices)
            => Dices = dices;
    }
}
