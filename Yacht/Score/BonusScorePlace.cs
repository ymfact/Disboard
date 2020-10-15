using System.Collections.Generic;
using System.Linq;

namespace Yacht
{
    class BonusScorePlace : IScorePlace
    {
        readonly IReadOnlyCollection<IScorePlace> _upper;

        public BonusScorePlace(IReadOnlyCollection<IScorePlace> upper)
        {
            _upper = upper;
        }

        public string Initial => "";

        public string Name => "Bonus";

        public string Desc => "+35pts if ≥ 63pts";

        public bool IsOpen => false;

        public int CurrentScore
        {
            get
            {
                if (_upper.Sum(place => place.CurrentScore) >= 63)
                {
                    return 35;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string CurrentScoreString
        {
            get
            {
                int currentScore = CurrentScore;
                if (currentScore > 0)
                {
                    return currentScore.ToString();
                }
                else
                {
                    return $"{_upper.Sum(place => place.CurrentScore)}/63";
                }
            }
        }

        public int CalculateScore(int[] dices) => 0;

        public void Submit(int[] dices)
            => throw new System.InvalidOperationException();
    }
}
