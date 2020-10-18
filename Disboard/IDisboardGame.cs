using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Disboard
{
    public interface IDisboardGame
    {
        internal bool IsDebug { get; }
        internal ConcurrentQueue<Task> MessageQueue { get; }

        IReadOnlyList<DisboardPlayer> InitialPlayers { get; }

        void Start();
        void OnGroup(DisboardPlayer author, string message);
    }
}
