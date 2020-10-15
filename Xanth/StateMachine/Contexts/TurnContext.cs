using System;
using System.Collections.Generic;
using System.Linq;

namespace Xanth
{
    class TurnContext
    {
        public int PlayerIndex { get; }
        public int[] Dices { get; }
        public int RemainReroll { get; }
        public int RemainMove { get; }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollFourDices => Enumerable.Range(0, 4).Select(_ => RollDice).ToArray();

        TurnContext(
            int playerIndex,
            int[] dices,
            int remainReroll,
            int remainMove
            )
        {
            PlayerIndex = playerIndex;
            Dices = dices;
            RemainReroll = remainReroll;
            RemainMove = remainMove;
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
            => new TurnContext(
                playerIndex: nextPlayerIndex,
                dices: RollFourDices,
                remainReroll: 3,
                remainMove: 4
                );

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
            newDices.AddRange(Enumerable.Range(0, 4 - newDices.Count).Select(_ => RollDice));

            return new TurnContext(
                playerIndex: PlayerIndex,
                dices: newDices.ToArray(),
                remainReroll: dicesToReroll.Count - 1,
                remainMove: dicesToReroll.Count
                );
        }

        public TurnContext OnMove()
            => new TurnContext(
                playerIndex: PlayerIndex,
                dices: Dices,
                remainReroll: 0,
                remainMove: RemainMove - 1
                );
    }
}
