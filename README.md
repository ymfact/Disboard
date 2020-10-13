# Disboard

### Platform of Online Text Board Game using Discord

A simple example:

```csharp
using Disboard;
using System.Threading.Tasks;

class Echo : IGame
{
    SendType Send { get; }
    public Echo(GameInitializeData initializer) => Send = initializer.Send;
    public Task Start() => Send("`Echo Started.`");
    public Task OnGroup(Player player, string message) => Send(message);
}

class Program
{
    static void Main()
    {
        var disboard = new Disboard(_ => new Echo(_));
        disboard.Run("TOKEN").GetAwaiter().GetResult();
    }
}
```

서버에 봇을 추가한 뒤, `BOT start`를 입력하면 해당 채널에 게임이 생성됩니다.

플레이어 간 공유되지 않는 내용은 DM을 활용하여 구현합니다.

봇을 실행하려면 토큰을 발급해야 합니다. https://discordpy.readthedocs.io/en/latest/discord.html 를 참고하세요.

###### 게임에 참가하는 인원은 게임 시작 후 변경할 수 없습니다. 게임에 참가하지 않는 유저는 `BOT restart` 명령어 외에는 게임에 영향을 주지 않습니다. `IGameUsesDM`을 상속하면 DM을 사용할 수 있습니다. DM은 하나의 게임으로만 전달되며, 플레이어는 `BOT restoredm` 명령어를 이용해 DM이 어느 게임으로 연결될지 선택합니다. 채널 주제(Topic)에 대소문자 무관 `debug`를 포함시키면 봇을 실행할 때마다 게임이 시작됩니다.

## Todo

- 텍스트 그리드를 이미지로 업로드
