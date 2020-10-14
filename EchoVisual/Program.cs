namespace EchoVisual
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard(_ => new EchoVisual(_));
            disboard.Run("TOKEN");
        }
    }
}
