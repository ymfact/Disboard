namespace Yacht
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard<YachtFactory>();
            disboard.Run(Token.token);
        }
    }
}
