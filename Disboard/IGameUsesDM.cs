using System.Threading.Tasks;

namespace Disboard
{
    public interface IGameUsesDM : IGame
    {
        public Task OnDM(Player author, string message, SendType reply);
    }
}
