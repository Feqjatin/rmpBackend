using System.Collections.Concurrent;

namespace rmpBackend.Queue
{
    public class InMemoryQueue
    {
         public static ConcurrentQueue<List<int>> Queue = new();
    }
}
