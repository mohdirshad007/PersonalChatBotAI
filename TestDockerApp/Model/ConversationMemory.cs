using System.Collections.Concurrent;

namespace TestDockerApp.Services
{
    public record ConversationEntry(string Role, string Content);

    public class ConversationMemory
    {
        private readonly ConcurrentDictionary<string, List<ConversationEntry>> _store = new();

        public List<ConversationEntry>? Get(string sessionId)
        {
            _store.TryGetValue(sessionId, out var list);
            return list;
        }

        public void Append(string sessionId, ConversationEntry entry)
        {
            var list = _store.GetOrAdd(sessionId, _ => new List<ConversationEntry>());
            lock (list)
            {
                list.Add(entry);
                // optional: limit history length
                if (list.Count > 20)
                {
                    list.RemoveRange(0, list.Count - 20);
                }
            }
        }

        public void Clear(string sessionId) => _store.TryRemove(sessionId, out _);
    }
}
