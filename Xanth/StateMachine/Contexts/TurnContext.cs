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
        public bool IsStuckInThisTurn { get; }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollFourDices => Enumerable.Range(0, 4).Select(_ => RollDice).ToArray();

        TurnContext(
            int playerIndex,
            int[] dices,
            int remainReroll,
            int remainMove,
            bool isStuckInThisTurn
            )
        {
            PlayerIndex = playerIndex;
            Dices = dices;
            RemainReroll = remainReroll;
            RemainMove = remainMove;
            IsStuckInThisTurn = isStuckInThisTurn;
        }

        public static TurnContext New(BoardContext board)
            => Next_(
                board: board,
                nextPlayerIndex: 0
                );

        public TurnContext Next(BoardContext board, int nextPlayerIndex)
            => Next_(
                board: board,
                nextPlayerIndex: nextPlayerIndex
                );

        static TurnContext Next_(BoardContext board, int nextPlayerIndex)
        {
            var newDices = RollFourDices;
            return new TurnContext(
                playerIndex: nextPlayerIndex,
                dices: newDices,
                remainReroll: 3,
                remainMove: 4,
                isStuckInThisTurn: board.IsStuck(nextPlayerIndex, newDices)
                );
        }

        public TurnContext Reroll(BoardContext board, IList<int> dicesToReroll)
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
            var newDicesArray = newDices.ToArray();

            return new TurnContext(
                playerIndex: PlayerIndex,
                dices: newDicesArray,
                remainReroll: dicesToReroll.Count - 1,
                remainMove: dicesToReroll.Count,
                isStuckInThisTurn: IsStuckInThisTurn && board.IsStuck(PlayerIndex, newDicesArray)
                );
        }

        public TurnContext OnMove()
            => new TurnContext(
                playerIndex: PlayerIndex,
                dices: Dices,
                remainReroll: 0,
                remainMove: RemainMove - 1,
                isStuckInThisTurn: false
                );
    }
}
