namespace Xanth
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard<XanthFactory>();
            disboard.Run(Token.token);
        }
    }
}
