using Disboard;

namespace MyGame
{
    class MyPlayer
    {
        public DisboardPlayer Disboard { get; }
        public MyPlayer NextPlayer { get; set; }
        public MyPlayer(DisboardPlayer disboardPlayer)
        {
            Disboard = disboardPlayer;
        }
    }
}
