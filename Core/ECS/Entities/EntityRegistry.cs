namespace Vint.Core.ECS.Entities;

public static class EntityRegistry {
    static long _lastId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    static Dictionary<long, IEntity> Entities { get; } = new();


    public static long FreeId => Interlocked.Increment(ref _lastId);

    public static void Add(IEntity entity) {
        lock (Entities) {
            if (Entities.ContainsKey(entity.Id))
                throw new ArgumentException($"Entity with id {entity.Id} already registered");

            Entities[entity.Id] = entity;
        }
    }

    public static IEntity Get(long id) => Entities[id];
}