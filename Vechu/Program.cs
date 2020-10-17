namespace Vechu
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard<VechuFactory>();
            disboard.Run(Token.token);
        }
    }
}
