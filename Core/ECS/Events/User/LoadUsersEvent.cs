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

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // bug: client crashes while scrolling friends list 
        UsersId.RemoveWhere(id => connection.SharedEntities.Any(entity => entity.Id == id));
        
        List<IPlayerConnection> playerConnections = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .ToList();
        
        using DbConnection db = new();

        foreach (IEntity user in UsersId
                     .Select(userId => db.Players.SingleOrDefault(player => player.Id == userId))
                     .Where(player => player != null)
                     .Select(player => playerConnections.SingleOrDefault(conn => conn.Player.Id == player!.Id)?.User ?? // user is online
                                       new UserTemplate().CreateFake(connection, player!))) // user is offline
            connection.ShareIfUnshared(user);

        connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}