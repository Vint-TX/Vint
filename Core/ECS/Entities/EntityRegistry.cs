using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public static class EntityRegistry {
    static long _lastId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    static ConcurrentDictionary<long, IEntity> Entities { get; } = new();
    static ConcurrentDictionary<long, IEntity> TempEntities { get; } = new();

    public static long FreeId => Interlocked.Increment(ref _lastId);

    public static void Add(IEntity entity) {
        if (!Entities.TryAdd(entity.Id, entity))
            throw new ArgumentException($"Entity with id {entity.Id} already registered");
    }

    public static void AddTemp(IEntity entity) {
        TempEntities.AddOrUpdate(entity.Id,
            _ => entity,
            (_, existingEntity) => {
                foreach (IPlayerConnection connection in existingEntity.SharedPlayers) {
                    connection.UnshareIfShared(existingEntity);
                    connection.ShareIfUnshared(entity);
                }

                return entity;
            });
    }

    public static void Remove(long id) {
        if (!Entities.TryRemove(id, out _))
            throw new ArgumentException($"Entity with id {id} is not registered");
    }

    public static bool TryRemoveTemp(long id) => TempEntities.TryRemove(id, out _);

    public static IEntity Get(long id) => Entities[id];

    public static bool TryGetTemp(long id, [NotNullWhen(true)] out IEntity? entity) => TempEntities.TryGetValue(id, out entity);
}