using Disboard;
using System.Collections.Generic;

namespace Yacht
{
    class InitialState : GameState
    {
        public static InitialState New(Game ctx, IReadOnlyList<Player> initialPlayers)
            => new InitialState(
                ctx: ctx,
                players: initialPlayers
                );

        public IReadOnlyList<Player> Players { get; }

        InitialState(Game ctx, IReadOnlyList<Player> players) : base(ctx)
            => Players = players;

        public override GameState OnStart()
            => TurnState.From(this);
    }
}
