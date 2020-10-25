using Disboard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vechu
{
    class TurnContext
    {
        public DisboardPlayer CurrentPlayer { get; }
        public int[] Dices { get; }
        public int RemainReroll { get; }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollTwoDices => Enumerable.Range(0, 2).Select(_ => RollDice).ToArray();

        TurnContext(
            DisboardPlayer currentPlayer,
            int[] dices,
            int remainReroll
            )
        {
            CurrentPlayer = currentPlayer;
            Dices = dices;
            RemainReroll = remainReroll;
        }

        public static TurnContext New(DisboardPlayer firstPlayer)
            => Next_(
                nextPlayer: firstPlayer
                );

        public TurnContext Next(DisboardPlayer nextPlayer)
            => Next_(
                nextPlayer
                );

        static TurnContext Next_(DisboardPlayer nextPlayer)
        {
            var newDices = RollTwoDices;
            return new TurnContext(
                currentPlayer: nextPlayer,
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
                currentPlayer: CurrentPlayer,
                dices: newDices.ToArray(),
                remainReroll: newDices[0] == newDices[1] ? dicesToReroll.Count : dicesToReroll.Count - 1
                );
        }
    }
}
