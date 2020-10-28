using Disboard;

namespace MyGame
{
    class Program
    {
        static void Main()
        {
            var disboard = new Disboard<MyGameFactory>();
            disboard.Run(Token.token);
        }
    }

}

 * disboard.Run("Token.token");의 두 큰 따옴표를 제거하고,
 * 아래 내용과 같은 Token.cs를 생성하세요.

static class Token
{
    public const string token = "이곳에 붙여넣기 하세요";
}

 */