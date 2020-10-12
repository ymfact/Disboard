using System.Threading.Tasks;

namespace Disboard
{
    public interface IGame
    {
        public Task Start();

        public Task OnGroup(User.IdType authorId, string message);
        public bool AcceptsOnGroup(User.IdType authorId, string message);
    }
}
