using System.Threading.Tasks;

namespace Disboard
{
    public abstract class Game : GameContext
    {
        public Game(GameInitializeData initData) : base(initData) { }
        public abstract Task Start();
        public abstract Task OnGroup(Player author, string message);
    }
}
