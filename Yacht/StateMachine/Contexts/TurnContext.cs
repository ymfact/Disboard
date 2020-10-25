using Disboard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yacht
{
    class TurnContext
    {
        public DisboardPlayer CurrentPlayer { get; }
        public int[] CurrentDices { get; }
        public int CurrentRemainReroll { get; }

        TurnContext(DisboardPlayer currentPlayer, int[] currentDices, int currentRemainReroll)
        {
            CurrentPlayer = currentPlayer;
            CurrentDices = currentDices;
            CurrentRemainReroll = currentRemainReroll;
        }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollFiveDices => Enumerable.Range(0, 5).Select(_ => RollDice).ToArray();

        public static TurnContext New(DisboardPlayer firstPlayer)
            => Next_(
                nextPlayer: firstPlayer
                );

        public TurnContext Next(DisboardPlayer nextPlayer)
            => Next_(nextPlayer);

        static TurnContext Next_(DisboardPlayer nextPlayer)
            => new TurnContext(
                currentPlayer: nextPlayer,
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
                currentPlayer: CurrentPlayer,
                currentDices: newDices.ToArray(),
                currentRemainReroll: CurrentRemainReroll - 1
                );
        }
    }
}
