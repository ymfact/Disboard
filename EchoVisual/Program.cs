namespace EchoVisual
{
    class Program
    {
        static void Main()
        {
            var disboard = new Disboard.Disboard<EchoVisualFactory>();
            disboard.Run("Token.token");
        }
    }
}

/* 1. 위 코드의 disboard.Run("Token.token");의 두 큰 따옴표를 제거합니다.
 * https://discordpy.readthedocs.io/en/latest/discord.html
 * 2. 위 페이지를 따라 토큰을 발급받습니다.
 * 3. 토큰 발급 페이지 중간의 PRESENCE INTENT, SERVER MEMBERS INTENT 스위치를 켜줍니다.
 * 4. 아래 내용과 같은 Token.cs를 생성하세요.

static class Token
{
    public const string token = "이곳에 붙여넣기 하세요";
}

 */