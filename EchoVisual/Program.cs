namespace EchoVisual
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard<EchoVisualFactory>();
            disboard.Run("TOKEN");
        }
    }
}
