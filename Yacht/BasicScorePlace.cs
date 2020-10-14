using System;

namespace Yacht
{
    class BasicScorePlace : IScorePlace
    {
        int[] _dices = { };

        readonly Func<int[], int> _scoreCalculator;

        public BasicScorePlace(string initial, string name, string desc, Func<int[], int> scoreCalculator)
        {
            Initial = initial;
            Name = name;
            Desc = desc;
            _scoreCalculator = scoreCalculator;
        }

        public string Initial { get; }

        public string Name { get; }

        public string Desc { get; }

        public bool IsOpen
            => _dices.Length == 0;

        public virtual int CurrentScore
        {
            get
            {
                if (_dices.Length == 0)
                {
                    return 0;
                }
                else
                {
                    return CalculateScore(_dices);
                }
            }
        }

        public string CurrentScoreString
            => CurrentScore.ToString();

        public int CalculateScore(int[] dices)
            => _scoreCalculator(dices);

        public void Submit(int[] dices)
            => _dices = dices;
    }
}
