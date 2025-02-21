using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Commands;

namespace Vint.Core.Utils;

public static class PlayerConnectionUtils {
    public static async Task Share(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.Share(entity);
    }

    public static async Task Share(this IEnumerable<IPlayerConnection> connections, params IEnumerable<IEntity> entities) {
        entities = entities as IEntity[] ?? entities.ToArray();

        foreach (IPlayerConnection connection in connections)
            await connection.Share(entities);
    }

    public static async Task ShareIfUnshared(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.ShareIfUnshared(entity);
    }

    public static async Task ShareIfUnshared(this IEnumerable<IPlayerConnection> connections, params IEnumerable<IEntity> entities) {
        entities = entities as IEntity[] ?? entities.ToArray();

        foreach (IPlayerConnection connection in connections)
            await connection.ShareIfUnshared(entities);
    }

    public static async Task Unshare(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.Unshare(entity);
    }

    public static async Task Unshare(this IEnumerable<IPlayerConnection> connections, params IEnumerable<IEntity> entities) {
        entities = entities as IEntity[] ?? entities.ToArray();

        foreach (IPlayerConnection connection in connections)
            await connection.Unshare(entities);
    }

    public static async Task UnshareIfShared(this IPlayerConnection connection, params IEnumerable<IEntity> entities) {
        foreach (IEntity entity in entities)
            await connection.UnshareIfShared(entity);
    }

    public static async Task UnshareIfShared(this IEnumerable<IPlayerConnection> connections, params IEnumerable<IEntity> entities) {
        entities = entities as IEntity[] ?? entities.ToArray();

        foreach (IPlayerConnection connection in connections)
            await connection.UnshareIfShared(entities);
    }

    public static async Task Send(this IEnumerable<IPlayerConnection> connections, IEvent @event) {
        foreach (IPlayerConnection connection in connections)
            await connection.Send(@event);
    }

    public static async Task Send(this IEnumerable<IPlayerConnection> connections, IEvent @event, params IEnumerable<IEntity> entities) {
        entities = entities as IEntity[] ?? entities.ToArray();

        foreach (IPlayerConnection connection in connections)
            await connection.Send(@event, entities);
    }

    public static async Task Send(this IEnumerable<IPlayerConnection> connections, ICommand command) {
        foreach (IPlayerConnection connection in connections)
            await connection.Send(command);
    }
}

public static class PlayerWithConnectionUtils {
    public static async Task Share(this IWithConnection player, IEntity entity) =>
        await player.Connection.Share(entity);

    public static async Task Share(this IWithConnection player, params IEnumerable<IEntity> entities) =>
        await player.Connection.Share(entities);

    public static async Task Share(this IEnumerable<IWithConnection> players, params IEnumerable<IEntity> entities) =>
        await players.Select(player => player.Connection).Share(entities);

    public static async Task ShareIfUnshared(this IWithConnection player, IEntity entity) =>
        await player.Connection.ShareIfUnshared(entity);

    public static async Task ShareIfUnshared(this IWithConnection player, params IEnumerable<IEntity> entities) =>
        await player.Connection.ShareIfUnshared(entities);

    public static async Task ShareIfUnshared(this IEnumerable<IWithConnection> players, params IEnumerable<IEntity> entities) =>
        await players.Select(player => player.Connection).ShareIfUnshared(entities);

    public static async Task Unshare(this IWithConnection player, IEntity entity) =>
        await player.Connection.Unshare(entity);

    public static async Task Unshare(this IWithConnection player, params IEnumerable<IEntity> entities) =>
        await player.Connection.Unshare(entities);

    public static async Task Unshare(this IEnumerable<IWithConnection> players, params IEnumerable<IEntity> entities) =>
        await players.Select(player => player.Connection).Unshare(entities);

    public static async Task UnshareIfShared(this IWithConnection player, IEntity entity) =>
        await player.Connection.UnshareIfShared(entity);

    public static async Task UnshareIfShared(this IWithConnection player, params IEnumerable<IEntity> entities) =>
        await player.Connection.UnshareIfShared(entities);

    public static async Task UnshareIfShared(this IEnumerable<IWithConnection> players, params IEnumerable<IEntity> entities) =>
        await players.Select(player => player.Connection).UnshareIfShared(entities);

    public static async Task Send(this IWithConnection player, IEvent @event) =>
        await player.Connection.Send(@event);

    public static async Task Send(this IWithConnection player, IEvent @event, params IEnumerable<IEntity> entities) =>
        await player.Connection.Send(@event, entities);

    public static async Task Send(this IEnumerable<IWithConnection> players, IEvent @event) =>
        await players.Select(player => player.Connection).Send(@event);

    public static async Task Send(this IEnumerable<IWithConnection> players, IEvent @event, params IEnumerable<IEntity> entities) =>
        await players.Select(player => player.Connection).Send(@event, entities);

    public static async Task Send(this IEnumerable<IWithConnection> players, ICommand command) =>
        await players.Select(player => player.Connection).Send(command);
}
