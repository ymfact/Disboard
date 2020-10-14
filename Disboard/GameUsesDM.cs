using System.Threading.Tasks;

namespace Disboard
{
    public abstract class GameUsesDM : Game
    {
        public GameUsesDM(GameInitializeData initData) : base(initData) { }
        public abstract Task OnDM(Player author, string message);
    }
}
