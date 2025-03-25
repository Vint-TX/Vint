using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Entities;

public static class UserRegistry {
    static ConcurrentDictionary<long, UserContainer> Users { get; } = [];

    public static bool TryGetContainer(long userId, [NotNullWhen(true)] out UserContainer? userContainer) =>
        Users.TryGetValue(userId, out userContainer);

    public static UserContainer GetContainer(long id) => Users[id];

    public static UserContainer GetOrCreateContainer(long id, Player player) =>
        Users.GetOrAdd(id, userId => new UserContainer(userId, player));
}

public class UserContainer(
    long id,
    Player player
) {
    Lazy<IEntity> EntityLazy { get; } = new(() => new UserTemplate().Create(player), LazyThreadSafetyMode.ExecutionAndPublication);
    ConcurrentDictionary<IPlayerConnection, uint> ConnectionToShareCount { get; } = [];

    public long Id { get; } = id;
    public IEntity Entity => EntityLazy.Value;

    public async Task ShareTo(IPlayerConnection connection) {
        uint newCount = ConnectionToShareCount.AddOrUpdate(connection, 1, (_, count) => checked(count + 1));

        if (newCount > 1) return;

        await connection.Share(Entity);
    }

    public async Task UnshareFrom(IPlayerConnection connection) {
        uint newCount = ConnectionToShareCount.AddOrUpdate(connection, _ => throw new KeyNotFoundException(), (_, count) => checked(count - 1));

        if (newCount != 0) return;

        await connection.Unshare(Entity);
    }

    public async Task RemoveConnection(IPlayerConnection connection) {
        if (ConnectionToShareCount.TryRemove(connection, out uint count) && count > 0)
            await connection.Unshare(Entity);
    }
}
