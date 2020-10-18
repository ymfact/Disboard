using Disboard;

class Program
{
    static void Main()
    {
        var disboard = new Disboard<DisboardGameFactory<Echo>>();
        disboard.Run("TOKEN");
    }
}
