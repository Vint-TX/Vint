using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1510640414175)]
public class RejectInviteToSquadEvent : IServerEvent {
    public long FromUserId { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        Task.FromResult(SquadRegistry.Invites.Remove(FromUserId, connection.UserContainer.Id));
}
