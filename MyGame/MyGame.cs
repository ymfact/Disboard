using Disboard;
class MyGame : DisboardGame
{
    public MyGame(DisboardGameInitData initData) : base(initData)
    {
        // 게임을 초기화하는 로직을 작성합니다.
        // 게임이 시작될 때 메시지를 보내거나, 또는 즉시 게임을 종료하는 작업은 이곳이 아닌 Start에서 해야 합니다.

        // InitialPlayers에는 게임에 참가하는 인원들이 있습니다.
        DisboardPlayer firstPlayer = InitialPlayers[0];
    }

    public override void Start()
    {
        // 게임이 시작되었을 때의 로직을 입력합니다.

        // Send는 그룹 채팅에 메시지를 보내는 함수입니다.
        // SendImage, SendImages 함수도 사용할 수 있습니다.

        Send("`게임이 시작되었습니다.`");

        // 표를 그리려면, 예제 프로젝트 Vechu의
        // BoardContext.GetBoardGrid
        // TurnState.PrintTurn
        // 를 참고하세요.
    }

    public override void OnGroup(DisboardPlayer player, string message)
    {
        // 메시지를 받았을 때의 로직을 입력합니다.

        // 게임을 종료하려면 OnFinish()를 호출합니다.
        // 인원 수가 안맞는 등의 이유로 게임을 시작할 수 없는 경우, OnFinish()를 생성자가 아닌 Start에서 호출해야 합니다.
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