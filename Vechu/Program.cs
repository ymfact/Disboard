namespace Vechu
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard(_ => new Vechu(_));
            disboard.Run(Token.token);
        }
    }
}
