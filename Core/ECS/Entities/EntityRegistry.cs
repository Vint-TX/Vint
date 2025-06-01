using System.Collections.Concurrent;

namespace Vint.Core.ECS.Entities;

public static class EntityRegistry {
    static long _lastId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    static ConcurrentDictionary<long, IEntity> Entities { get; } = new();

    public static long GenerateId() => Interlocked.Increment(ref _lastId);

    public static void Add(IEntity entity) {
        if (!Entities.TryAdd(entity.Id, entity))
            throw new ArgumentException($"Entity with id {entity.Id} already registered");
    }

    public static void Remove(long id) {
        if (!TryRemove(id))
            throw new ArgumentException($"Entity with id {id} is not registered");
    }

    public static bool TryRemove(long id) => Entities.TryRemove(id, out _);

    public static IEntity Get(long id) => Entities[id];

    public static IEntity? TryGet(long id) => Entities.GetValueOrDefault(id);
}
