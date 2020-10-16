using Disboard;
using System.Collections.Generic;

namespace Vechu
{
    class InitialState : GameState
    {
        public static InitialState New(GameContext ctx, IReadOnlyList<Player> initialPlayers)
            => new InitialState(
                   ctx: ctx,
                   players: initialPlayers
                   );

        public IReadOnlyList<Player> Players { get; }

        InitialState(GameContext ctx, IReadOnlyList<Player> players) : base(ctx)
            => Players = players;

        public override GameState OnStart()
            => TurnState.From(this);
    }
}
