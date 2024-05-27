using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.Utils;

public static class PlayerConnectionUtils {
    public static Task Share(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.Share(entities as IEnumerable<IEntity>);

    public static Task ShareIfUnshared(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.ShareIfUnshared(entities as IEnumerable<IEntity>);

    public static Task Unshare(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.Unshare(entities as IEnumerable<IEntity>);

    public static Task UnshareIfShared(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.UnshareIfShared(entities as IEnumerable<IEntity>);

    public static async Task Share(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.Share(entity);
    }

    public static async Task ShareIfUnshared(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.ShareIfUnshared(entity);
    }

    public static async Task Unshare(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.Unshare(entity);
    }

    public static async Task UnshareIfShared(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.UnshareIfShared(entity);
    }
}
