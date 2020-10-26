using Disboard;
using System.Collections.Generic;

namespace Xanth
{
    class Player
    {
        public DisboardPlayer Disboard { get; }
        public Player NextPlayer
        {
            get
            {
                while (_nextPlayer.IsDropped && _nextPlayer != this)
                    _nextPlayer = _nextPlayer._nextPlayer;

                return _nextPlayer;
            }
            set => _nextPlayer = value;
        }
        Player _nextPlayer;
        public Marker Marker { get; set; }
        public bool RemainFirstMoveBonus { get; set; }
        public bool IsDropped { get; set; } = false;
        public string Color { get; set; }
        public string DarkColor { get; set; }
        public Player(IReadOnlyList<DisboardPlayer> players, DisboardPlayer player, Marker marker)
        {
            _nextPlayer = this;
            Marker = marker;
            Disboard = player;

            var colors = players.Count == 2 ? ColorsForTwo : ColorsForFour;
            var darkColors = players.Count == 2 ? DarkColorsForTwo : DarkColorsForFour;

            var playerIndex = players.FindIndex(_ => _ == player)!.Value;
            Color = colors[playerIndex];
            DarkColor = darkColors[playerIndex];

            RemainFirstMoveBonus = players.Count == 2;
        }
        static readonly IReadOnlyList<string> ColorsForTwo = new[] { "#005AC2", "#C21000" };
        static readonly IReadOnlyList<string> DarkColorsForTwo = new[] { "#01326B", "#6B0900" };
        static readonly IReadOnlyList<string> ColorsForFour = new[] { "#C21200", "#C2A813", "#13C264", "#190AC2" };
        static readonly IReadOnlyList<string> DarkColorsForFour = new[] { "#6B0A00", "#6B5D0B", "#0B6B37", "#0E056B" };
    }
}
