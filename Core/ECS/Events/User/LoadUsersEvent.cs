using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555246853)]
public class LoadUsersEvent : IServerEvent {
    public long RequestEntityId { get; private set; }
    public HashSet<long> UsersId { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();

        List<IPlayerConnection> playerConnections = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .ToList();

        foreach (IEntity user in UsersId
                     .Select(userId => db.Players.SingleOrDefault(player => player.Id == userId))
                     .Where(player => player != null)
                     .Select(player => connection.SharedEntities.SingleOrDefault(entity => entity.Id == player!.Id) ??
                                       playerConnections
                                           .SingleOrDefault(conn =>
                                               conn.Player.Id == player!.Id)?.User ??
                                       new UserTemplate().CreateFake(connection, player!))) connection.ShareIfUnshared(user);

        connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}