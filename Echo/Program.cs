using Disboard;

class Program
{
    static void Main()
    {
        disboard.Run("TOKEN");
        var disboard = new Disboard<GameFactory<Echo>>();
    }
}
