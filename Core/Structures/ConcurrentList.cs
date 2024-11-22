using System.Collections;
using JetBrains.Annotations;

namespace Vint.Core.Structures;

public class ConcurrentList<T>(
    List<T> inner
) : IList<T> {
    readonly Lock _lock = new();

    public ConcurrentList() : this([]) { }

    IEnumerator IEnumerable.GetEnumerator() {
        lock (_lock) {
            return inner
                .ToList()
                .GetEnumerator();
        }
    }

    [MustDisposeResource]
    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        lock (_lock) {
            return inner
                .ToList()
                .GetEnumerator();
        }
    }

    public int Count {
        get {
            lock (_lock) {
                return inner.Count;
            }
        }
    }

    public bool IsReadOnly => ((ICollection<T>)inner).IsReadOnly;

    public void Add(T item) {
        lock (_lock) {
            inner.Add(item);
        }
    }

    public void Clear() {
        lock (_lock) {
            inner.Clear();
        }
    }

    public bool Contains(T item) {
        lock (_lock) {
            return inner.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex) {
        lock (_lock) {
            inner.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(T item) {
        lock (_lock) {
            return inner.Remove(item);
        }
    }

    public int IndexOf(T item) {
        lock (_lock) {
            return inner.IndexOf(item);
        }
    }

    public void Insert(int index, T item) {
        lock (_lock) {
            inner.Insert(index, item);
        }
    }

    public void RemoveAt(int index) {
        lock (_lock) {
            inner.RemoveAt(index);
        }
    }

    public T this[int index] {
        get {
            lock (_lock) {
                return inner[index];
            }
        }
        set {
            lock (_lock) {
                inner[index] = value;
            }
        }
    }

    public int RemoveAll(Predicate<T> match) {
        lock (_lock) {
            return inner.RemoveAll(match);
        }
    }
}
