using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Disboard
{
    public interface IGame
    {
        internal bool IsDebug { get; }
        internal ConcurrentQueue<Task> MessageQueue { get; }

        IReadOnlyList<Player> InitialPlayers { get; }

        void Start();
        void OnGroup(Player author, string message);
    }
}
