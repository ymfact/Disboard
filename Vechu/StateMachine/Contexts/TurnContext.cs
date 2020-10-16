using System;
using System.Collections.Generic;
using System.Linq;

namespace Vechu
{
    class TurnContext
    {
        public int PlayerIndex { get; }
        public int[] Dices { get; }
        public int RemainReroll { get; }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollTwoDices => Enumerable.Range(0, 2).Select(_ => RollDice).ToArray();

        TurnContext(
            int playerIndex,
            int[] dices,
            int remainReroll
            )
        {
            PlayerIndex = playerIndex;
            Dices = dices;
            RemainReroll = remainReroll;
        }

        public static TurnContext New()
            => Next_(
                nextPlayerIndex: 0
                );

        public TurnContext Next(int nextPlayerIndex)
            => Next_(
                nextPlayerIndex
                );

        static TurnContext Next_(int nextPlayerIndex)
        {
            var newDices = RollTwoDices;
            return new TurnContext(
                playerIndex: nextPlayerIndex,
                dices: newDices,
                remainReroll: newDices[0] == newDices[1] ? 2 : 1
                );
        }

        public TurnContext Reroll(IList<int> dicesToReroll)
        {
            var newDices = Dices.ToList(); //copy
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
            newDices.AddRange(Enumerable.Range(0, 2 - newDices.Count).Select(_ => RollDice));

            return new TurnContext(
                playerIndex: PlayerIndex,
                dices: newDices.ToArray(),
                remainReroll: newDices[0] == newDices[1] ? RemainReroll : RemainReroll - 1
                );
        }
    }
}
