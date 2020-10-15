using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yacht
{
    class TurnContext
    {
        public int CurrentPlayerIndex { get; }
        public int[] CurrentDices { get; }
        public int CurrentRemainReroll { get; }

        TurnContext(int currentPlayerIndex, int[] currentDices, int currentRemainReroll)
        {
            CurrentPlayerIndex = currentPlayerIndex;
            CurrentDices = currentDices;
            CurrentRemainReroll = currentRemainReroll;
        }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollFiveDices => Enumerable.Range(0, 5).Select(_ => RollDice).ToArray();

        public static TurnContext New()
            => Next_(
                nextPlayerIndex: 0
                );

        public TurnContext Next(int nextPlayerIndex)
            => Next_(nextPlayerIndex);

        static TurnContext Next_(int nextPlayerIndex)
            => new TurnContext(
                currentPlayerIndex: nextPlayerIndex,
                currentDices: RollFiveDices,
                currentRemainReroll: 2
                );

        public TurnContext Reroll(IEnumerable<int> dicesToReroll)
        {
            var newDices = CurrentDices.ToList(); //copy
            foreach (int diceToReroll in dicesToReroll)
            {
                if (newDices.Contains(diceToReroll))
                {
                    newDices.RemoveAt(newDices.LastIndexOf(diceToReroll));
                }
                else
                {
                    throw new System.FormatException();
                }
            }
            newDices.AddRange(Enumerable.Range(0, 5 - newDices.Count).Select(_ => RollDice));

            return new TurnContext(
                currentPlayerIndex: CurrentPlayerIndex,
                currentDices: newDices.ToArray(),
                currentRemainReroll: CurrentRemainReroll - 1
                );
        }
    }
}
