namespace Yahtzee
{
    class Program
    {
        static void Main()
        {
            var disboard = new Disboard.Disboard(_ => new Yahtzee(_));
            disboard.Run(Token.token);
        }
    }
}
