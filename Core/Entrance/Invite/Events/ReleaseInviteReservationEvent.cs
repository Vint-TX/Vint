using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Invite.Events;

[ProtocolId(1444892358143)]
public class ReleaseInviteReservationEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        Task.FromResult(connection.Invite = null);
}
