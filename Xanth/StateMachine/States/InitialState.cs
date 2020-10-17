using Disboard;
using System.Collections.Generic;

namespace Xanth
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
        {
            if (Players.Count == 2 || Players.Count == 4)
            {
                return TurnState.From(this);
            }
            else
            {
                ctx.Send("`Xanth는 2인, 4인으로만 플레이 가능합니다.`");
                ctx.OnFinish();
                return this;
            }
        }
    }
}
