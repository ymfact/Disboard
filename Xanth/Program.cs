using System;

namespace Xanth
{
    class Program
    {
        [System.STAThread()]
        static void Main()
        {
            var disboard = new Disboard.Disboard(_ => new Xanth(_));
            disboard.Run(Token.token);
        }
    }
}
