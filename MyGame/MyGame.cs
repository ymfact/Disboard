using Disboard;
using System.Collections.Generic;
using System.Linq;

namespace MyGame
{
    class MyGame : DisboardGame
    {
        // 여기에 멤버변수를 선언하세요.
        readonly List<MyPlayer> _players;
        readonly Dictionary<DisboardPlayer, MyPlayer> _playerDict;

        MyPlayer _currentPlayer;

        public MyGame(DisboardGameInitData initData) : base(initData)
        {
            // 게임이 시작되었을 때의 로직을 입력합니다.


            // InitialPlayers에는 게임에 참가하는 인원들이 DisboardPlayer 타입으로 저장되어 있습니다.
            // 하지만 DisboardPlayer 타입에는 플레이어 이름 같은 것들만 있고, 게임에서 사용할 HP, MP와 같은 변수를 추가할 수 없습니다.

            // 그래서 플레이어가 가지는 정보를 관리할 수 있도록 MyPlayer 타입을 새롭게 정의하였습니다.
            // MyPlayer타입을 사용하려면, 기존 DisboardPlayer 타입으로 저장되어 있는 정보를 MyPlayer로 변환하는 과정이 필요합니다.

            _players = InitialPlayers.Select(disboardPlayer => new MyPlayer(disboardPlayer)).ToList();
            _playerDict = _players.ToDictionary(keySelector: player => player.Disboard);

            // 새로 정의한 MyPlayer들의 NextPlayer를 지정합니다.
            foreach (var (index, player) in _players.Enumerate())
            {
                int nextPlayerIndex = (index == _players.Count - 1) ? 0 : index + 1;
                player.NextPlayer = _players[nextPlayerIndex];
            }


            _currentPlayer = _players.First();
            // _currentPlayer = _players[0];
            // 위 두 줄은 같은 코드입니다.


            // Send는 그룹 채팅에 메시지를 보내는 함수입니다.
            // SendImage, SendImages 함수도 사용할 수 있습니다.
            Send($"첫번째 플레이어: {_currentPlayer.Disboard.Mention}");
        }

        public override void OnGroup(DisboardPlayer disboardAuthor, string message)
        {
            // 메시지를 받았을 때의 로직을 입력합니다.

            MyPlayer author = _playerDict[disboardAuthor];
            if (author == _currentPlayer)
            {
                // Send는 그룹 채팅에 메시지를 보내는 함수입니다.
                // SendImage, SendImages 함수도 사용할 수 있습니다.
                Send($"플레이어의 메시지: {message}");

                _currentPlayer = _currentPlayer.NextPlayer;

                Send($"다음 플레이어: {_currentPlayer.Disboard.Mention}");
            }


            // 게임을 종료하려면 OnFinish()를 호출합니다.
            // 인원 수가 안맞는 등의 이유로 게임을 시작할 수 없는 경우,
            // 생성자에서 OnFinish()를 호출하여 게임을 즉시 종료할 수도 있습니다.


            // 예제 프로젝트 Vechu를 확인하면 더 잘 활용할 수 있습니다.
            // 표를 그리려면 BoardContext.GetBoardGrid 를 참고하세요.
            // 다양한 출력 방법을 알아보려면 TurnState.PrintTurn 을 참고하세요.
        }

        public override void OnTick()
        {
            // 이 함수는 0.1초마다 호출됩니다.
            // 사용하지 않는 경우 함수를 지워도 좋습니다.
        }
    }

    class MyGameFactory : IDisboardGameFactory
    {
        public DisboardGame New(DisboardGameInitData initData) => new MyGame(initData);
        public void OnHelp(DisboardChannel channel)
        {
            // BOT help가 호출되었을 때 반응하는 로직을 작성합니다.

            string helpString = "예제 프로젝트입니다.";
            channel.Send(helpString);
        }
    }
}
