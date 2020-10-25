using Disboard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xanth
{
    class TurnContext
    {
        public Player CurrentPlayer { get; }
        public int[] Dices { get; }
        public int RemainReroll { get; }
        public int RemainMove { get; }
        public bool IsStuckInThisTurn { get; }

        protected static Random Random { get; } = new Random();
        protected static int RollDice => Random.Next(6) + 1;
        protected static int[] RollFourDices => Enumerable.Range(0, 4).Select(_ => RollDice).ToArray();

        TurnContext(
            Player currentPlayer,
            int[] dices,
            int remainReroll,
            int remainMove,
            bool isStuckInThisTurn
            )
        {
            CurrentPlayer = currentPlayer;
            Dices = dices;
            RemainReroll = remainReroll;
            RemainMove = remainMove;
            IsStuckInThisTurn = isStuckInThisTurn;
        }

        public static TurnContext New(BoardContext board)
            => Next_(
                board: board,
                nextPlayer: board.PlayerDict.First().Value
                );

        public TurnContext Next(BoardContext board)
            => Next_(
                board: board,
                nextPlayer: CurrentPlayer.GetNextPlayer(board)
                );

        static TurnContext Next_(BoardContext board, Player nextPlayer)
        {
            var newDices = RollFourDices;
            return new TurnContext(
                currentPlayer: nextPlayer,
                dices: newDices,
                remainReroll: 3,
                remainMove: 4,
                isStuckInThisTurn: board.IsStuck(nextPlayer, newDices)
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
                currentPlayer: CurrentPlayer,
                dices: newDicesArray,
                remainReroll: dicesToReroll.Count - 1,
                remainMove: dicesToReroll.Count,
                isStuckInThisTurn: board.IsStuck(CurrentPlayer, newDicesArray)
                );
        }

        public TurnContext OnMove()
            => new TurnContext(
                currentPlayer: CurrentPlayer,
                dices: Dices,
                remainReroll: 0,
                remainMove: RemainMove - 1,
                isStuckInThisTurn: false
                );
    }
}
