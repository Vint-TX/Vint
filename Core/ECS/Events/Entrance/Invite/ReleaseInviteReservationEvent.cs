using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Invite;

[ProtocolId(1444892358143)]
public class ReleaseInviteReservationEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        Task.FromResult(connection.Invite = null);
}
