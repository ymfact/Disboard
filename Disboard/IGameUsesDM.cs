using System.Threading.Tasks;
using static Disboard.GameInitializer;

namespace Disboard
{
    public interface IGameUsesDM : IGame
    {
        public Task OnDM(User.IdType authorId, string message, SendType reply);
    }
}
