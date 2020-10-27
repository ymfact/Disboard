namespace Vechu
{
    class Program
    {
        static void Main()
        {
            var disboard = new Disboard.Disboard<VechuFactory>();
            disboard.Run("Token.token");
        }
    }
}

/* https://discordpy.readthedocs.io/en/latest/discord.html
 * 위 페이지를 따라 토큰을 발급받은 뒤,
 * disboard.Run("Token.token");의 두 큰 따옴표를 제거하고,
 * 아래 내용과 같은 Token.cs를 생성하세요.

static class Token
{
    public const string token = "이곳에 붙여넣기 하세요";
}

 */