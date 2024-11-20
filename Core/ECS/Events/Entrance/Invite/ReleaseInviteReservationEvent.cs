using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Entrance.Invite;

[ProtocolId(1444892358143)]
public class ReleaseInviteReservationEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) =>
        Task.FromResult(connection.Invite = null);
}
