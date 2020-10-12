# Disboard

A simple example:

```csharp
using Disboard;
using System.Threading.Tasks;
using static Disboard.GameInitializer;

class Echo : IGame
{
    SendType Send { get; }
    public Echo(GameInitializer initializer) => Send = initializer.Send;
    public Task Start() => Send("`Echo Started.`");
    public Task OnGroup(User.IdType authorId, string message) => Send(message);
}

class Program
{
    static void Main()
    {
        var disboard = new Disboard.Disboard(_ => new Echo(_));
        disboard.Run("TOKEN").GetAwaiter().GetResult();
    }
}
```

서버에 봇을 추가한 뒤, BOT start를 입력하면 해당 채널에 게임이 생성됩니다.

게임이 IGameUsesDM을 상속하면, DM을 활용하는 게임을 만들 수도 있습니다.

채널 주제(Topic)에 debug가 있다면, 프로그램을 실행하자마자 게임이 시작됩니다. (대소문자무관)
