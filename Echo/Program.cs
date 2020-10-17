using Disboard;

class Program
{
    static void Main()
    {
        var disboard = new Disboard<GameFactory<Echo>>();
        disboard.Run("TOKEN");
    }
}
