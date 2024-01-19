using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.Utils;

public static class PlayerConnectionUtils {
    public static void Share(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.Share(entities as IEnumerable<IEntity>);

    public static void ShareIfUnshared(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.ShareIfUnshared(entities as IEnumerable<IEntity>);

    public static void Unshare(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.Unshare(entities as IEnumerable<IEntity>);

    public static void UnshareIfShared(this IPlayerConnection connection, params IEntity[] entities) =>
        connection.UnshareIfShared(entities as IEnumerable<IEntity>);

    public static void Share(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            connection.Share(entity);
    }

    public static void ShareIfUnshared(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            connection.ShareIfUnshared(entity);
    }

    public static void Unshare(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            connection.Unshare(entity);
    }

    public static void UnshareIfShared(this IPlayerConnection connection, IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            connection.UnshareIfShared(entity);
    }
}