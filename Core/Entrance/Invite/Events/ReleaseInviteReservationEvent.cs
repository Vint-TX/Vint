using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Invite;

[ProtocolId(1444892358143)]
public class ReleaseInviteReservationEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        Task.FromResult(connection.Invite = null);
}
