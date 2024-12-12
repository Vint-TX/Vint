using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Templates.User;

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
    public long Id { get; } = id;

    public IEntity Entity => EntityLazy.Value;
}
