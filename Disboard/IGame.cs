using System.Threading.Tasks;

namespace Disboard
{
    public interface IGame
    {
        public Task Start();
        public Task OnGroup(Player author, string message);
    }
}
