# Disboard

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

서버에 봇을 추가한 뒤, BOT start를 입력하면 해당 채널에 게임이 생성됩니다.

게임이 IGameUsesDM을 상속하면, DM을 활용하는 게임을 만들 수도 있습니다.

###### 게임에 참가하는 인원은 게임 시작 후 변경할 수 없습니다. 게임에 참가하지 않는 유저의 명령어는 게임에 영향을 주지 않습니다. 채널 주제(Topic)에 'debug'를 포함시키면 봇을 실행할 때마다 게임이 시작됩니다. (대소문자무관)
