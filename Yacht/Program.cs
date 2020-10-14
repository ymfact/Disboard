namespace Yacht
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard(_ => new Yacht(_));
            disboard.Run(Token.token);
        }
    }
}
