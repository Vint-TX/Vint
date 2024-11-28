using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;

namespace Vint.Core.Utils;

public static class PlayerConnectionUtils {
    public static async Task Share(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.Share(entity);
    }

    public static async Task ShareIfUnshared(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.ShareIfUnshared(entity);
    }

    public static async Task Unshare(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.Unshare(entity);
    }

    public static async Task UnshareIfShared(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.UnshareIfShared(entity);
    }
}
