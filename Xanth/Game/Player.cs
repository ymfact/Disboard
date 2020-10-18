using Disboard;
using System.Collections.Generic;

namespace Xanth
{
    class Player
    {
        public Disboard.DisboardPlayer Disboard { get; }
        public Marker Marker { get; set; }
        public bool NotMovedYet { get; set; } = true;
        public bool IsDropped { get; set; } = false;
        public string Color { get; set; }
        public string DarkColor { get; set; }
        public Player(IReadOnlyList<Disboard.DisboardPlayer> players, Disboard.DisboardPlayer player, Marker marker)
        {
            Marker = marker;
            Disboard = player;

            var colors = players.Count == 2 ? ColorsForTwo : ColorsForFour;
            var darkColors = players.Count == 2 ? DarkColorsForTwo : DarkColorsForFour;

            var playerIndex = players.FindIndex(_ => _ == player)!.Value;
            Color = colors[playerIndex];
            DarkColor = darkColors[playerIndex];
        }
        static IReadOnlyList<string> ColorsForTwo = new[] { "#005AC2", "#C21000" };
        static IReadOnlyList<string> DarkColorsForTwo = new[] { "#01326B", "#6B0900" };
        static IReadOnlyList<string> ColorsForFour = new[] { "#C21200", "#C2A813", "#13C264", "#190AC2" };
        static IReadOnlyList<string> DarkColorsForFour = new[] { "#6B0A00", "#6B5D0B", "#0B6B37", "#0E056B" };
    }
}
