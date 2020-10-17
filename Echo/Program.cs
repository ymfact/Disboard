class Program
{
    static void Main()
    {
        var disboard = new Disboard.Disboard<EchoFactory>();
        disboard.Run("TOKEN");
    }
}
