class Program
{
    static void Main()
    {
        var disboard = new Disboard.Disboard(_ => new Echo(_));
        disboard.Run("TOKEN");
    }
}