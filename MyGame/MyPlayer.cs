using Disboard;

namespace MyGame
{
    // Disboard가 제공하는 DisboardPlayer 타입에는 플레이어 이름 같은 것들만 있고, 게임에서 사용할 HP, MP와 같은 변수를 추가할 수 없습니다.
    // 그래서 플레이어가 가지는 정보를 관리할 수 있도록 MyPlayer 타입을 새롭게 정의하였습니다.

    class MyPlayer
    {
        // 플레이어가 가져야 할 정보를 이곳에 선언하면 간단하게 관리할 수 있습니다.

        public DisboardPlayer Disboard { get; }
        public MyPlayer NextPlayer { get; set; }

        public MyPlayer(DisboardPlayer disboardPlayer)
        {
            Disboard = disboardPlayer;
        }
    }
}
