using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Notification;

[ProtocolId(1454667308567)]
public class NotificationShownEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        await connection.Unshare(entities.Single());
}
