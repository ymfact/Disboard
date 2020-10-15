using Disboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yacht
{
    class InitialState : GameState
    {
        public static InitialState New(GameContext context, IReadOnlyList<Player> initialPlayers)
            => new InitialState(
                context: context,
                players: initialPlayers
                );

        public IReadOnlyList<Player> Players { get; }

        InitialState(GameContext context, IReadOnlyList<Player> players) : base(context)
            => Players = players;

        public override async Task<GameState> OnStart()
            => await TurnState.From(this);
    }
}
