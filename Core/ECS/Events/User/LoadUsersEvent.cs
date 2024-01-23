using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555246853)]
public class LoadUsersEvent : IServerEvent {
    public long RequestEntityId { get; private set; }
    public HashSet<long> UsersId { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.InLobby)
            UsersId.ExceptWith(connection.BattlePlayer!.Battle.Players.Select(battlePlayer => battlePlayer.PlayerConnection.User.Id));

        HashSet<IEntity> users = [];

        foreach (IEntity user in UsersId
                     .ToArray()
                     .Select(id => connection.SharedEntities.SingleOrDefault(entity => entity.Id == id)!)
                     .Where(user => user != null!)) {
            users.Add(user);
            connection.Unshare(user);
            UsersId.Remove(user.Id);
        }

        IPlayerConnection[] playerConnections = connection.Server.PlayerConnections
            .Where(conn => conn.IsOnline)
            .ToArray();

        using DbConnection db = new();

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (long userId in UsersId) {
            Player? player = db.Players.SingleOrDefault(player => player.Id == userId);
            if (player == null) continue;

            IEntity user = playerConnections.SingleOrDefault(conn => conn.Player.Id == userId)?.User ??
                           new UserTemplate().CreateFake(connection, player);

            users.Add(user);
        }

        connection.ShareIfUnshared(users);
        connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}