using Disboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xanth
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
        {
            if (Players.Count != 2)
            {
                ctx.Send("`Xanth는 2인으로만 플레이 가능합니다.`");
                ctx.OnFinish();
                return this;
            }

            return TurnState.From(this);
        }
    }
}
