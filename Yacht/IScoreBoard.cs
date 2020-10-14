using System.Collections.Generic;

namespace Yacht
{
    interface IScoreBoard
    {
        IReadOnlyDictionary<string, IScorePlace> Places { get; }
        int TotalScore { get; }

        void Submit(string initial, int[] dices);
    }
}