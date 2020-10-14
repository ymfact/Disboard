namespace YachtVisual
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard(_ => new YachtVisual(_));
            disboard.Run(Token.token);
        }
    }
}
