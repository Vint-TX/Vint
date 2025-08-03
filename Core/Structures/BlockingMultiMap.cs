using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Vint.Core.Structures;

[PublicAPI]
public class BlockingMultiMap<TKey, TValue> : Dictionary<TKey, HashSet<TValue>> where TKey : notnull {
    readonly Lock _lock = new();

    public bool Add(TKey key, TValue value) {
        lock (_lock)
            return TryGetValue(key, out HashSet<TValue>? values)
                ? values.Add(value)
                : TryAdd(key, [value]);
    }

    public bool TryRemove(TKey key, TValue value) {
        lock (_lock) {
            if (!TryGetValue(key, out HashSet<TValue>? values))
                return false;

            bool success = values.Remove(value);

            if (values.Count == 0)
                Remove(key); // maybe need to return false if not removed

            return success;
        }
    }

    public bool RemoveAll(TKey key, [NotNullWhen(true)] out HashSet<TValue>? values) {
        lock (_lock)
            return Remove(key, out values);
    }

    public int RemoveWhere(TKey key, Predicate<TValue> predicate) {
        lock (_lock) {
            if (!TryGetValue(key, out HashSet<TValue>? values))
                return 0;

            int removedCount = values.RemoveWhere(predicate);

            if (values.Count == 0)
                Remove(key);

            return removedCount;
        }
    }

    public int RemoveWhere(Predicate<TValue> predicate) {
        lock (_lock) {
            HashSet<TKey> keysToRemove = [];
            int removedCount = 0;

            foreach ((TKey key, HashSet<TValue> values) in this) {
                removedCount += values.RemoveWhere(predicate);

                if (values.Count == 0)
                    keysToRemove.Add(key);
            }

            foreach (TKey key in keysToRemove)
                Remove(key);

            return removedCount;
        }
    }

    public IEnumerable<TValue> GetAllValues() =>
        Keys.SelectMany(key => base[key]).Distinct();
}
