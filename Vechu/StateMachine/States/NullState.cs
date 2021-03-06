﻿using Disboard;

namespace Vechu
{
    class NullState : IGameState
    {
        public static NullState New => new NullState();
        NullState() { }
        public IGameState OnGroup(DisboardPlayer player, string message) => this;
    }
}
