using System.Threading.Tasks;

namespace Disboard
{
    public abstract class Game : GameContext
    {
        public Game(GameInitializeData initData) : base(initData) { }
        public abstract void Start();
        public abstract void OnGroup(Player author, string message);
    }
}
