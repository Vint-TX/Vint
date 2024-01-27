using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1451368548585)]
public class RequestLoadUserProfileEvent : IServerEvent {
    public long UserId { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();

        Player? player = db.Players.SingleOrDefault(player => player.Id == UserId);

        if (player == null) return;

        IEntity user = connection.SharedEntities.SingleOrDefault(entity => entity.Id == UserId) ??
                       connection.Server.PlayerConnections.Values
                           .Where(conn => conn.IsOnline)
                           .SingleOrDefault(conn => conn.Player.Id == UserId)?.User ??
                       new UserTemplate().CreateFake(connection, player);

        if (!user.SharedPlayers.Contains(connection)) connection.Share(user);
        connection.Send(new UserProfileLoadedEvent(), user);
    }
}