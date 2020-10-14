# Disboard

### Online Text-Based Board Game Framework using Discord

#### A simple example
```csharp
using Disboard;
using System.Threading.Tasks;

class Echo : Game
{
    public Echo(GameInitializeData initData) : base(initData) { }
    public override Task Start() => Send("`Echo Started.`");
    public override Task OnGroup(Player player, string message) => Send(message);
}

class Program
{
    static void Main()
    {
        var disboard = new Disboard(_ => new Echo(_));
        disboard.Run("TOKEN");
    }
}
```

서버에 봇을 추가한 뒤, `BOT start`를 입력하면 해당 채널에 게임이 생성됩니다.

플레이어 간 공유되지 않는 내용은 DM을 활용하여 구현합니다.

봇을 실행하려면 토큰을 발급해야 합니다. https://discordpy.readthedocs.io/en/latest/discord.html 를 참고하세요.

<hr/>

#### Game
게임에 참가하는 인원은 게임 시작 후 변경할 수 없습니다.
게임에 참가하지 않는 유저는 `BOT restart` 명령어 외에는 게임에 영향을 주지 않습니다.
클라이언트는 채널마다 서로 다른 게임을 동시에 진행할 수 있습니다.
한 플레이어는 여러 게임에 참가할 수 있습니다.

#### Image from WPF Controls
WPF 컨트롤을 사용하여 이미지를 그릴 수 있습니다. EchoVisual 프로젝트를 참고하세요.

WPF 컨트롤을 사용하려면 다음 작업을 통해 프로젝트가 WPF를 사용하도록 설정해야 합니다.
csproj 파일에 `<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">`를 사용하고, `<PropertyGroup>` `</PropertyGroup>` 내부에 `<UseWPF>true</UseWPF>`를 추가합니다.
Main 함수 상단에 `[System.STAThread()]`를 추가합니다.

#### Disgrid
Disgrid를 사용하면 텍스트로 이루어진 표를 간단히 만들 수 있습니다.
Yacht.cs의 GetBoardImage를 예제로 참고하세요.

#### DM
IGameUsesDM을 상속하면 DM을 사용할 수 있습니다.
플레이어의 비밀 입력이 필요할 때 봇은 플레이어에게 DM을 보냅니다.
그 후 다시 그룹 채널로 돌아가기 위해 서버 링크를 제공할 수도 있습니다.
플레이어가 작성하는 DM은 플레이어마다 하나의 게임으로만 전달되며, 플레이어는 `BOT restoredm` 명령어를 이용해 플레이어가 작성하는 DM이 어느 게임으로 전달될지 선택할 수 있습니다.

#### debug
채널 주제(Topic)에 대소문자 무관 `debug`를 포함시키면 봇을 실행할 때마다 게임이 시작됩니다.
