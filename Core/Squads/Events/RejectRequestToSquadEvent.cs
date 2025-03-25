using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1510641456884)]
public class RejectRequestToSquadEvent : IServerEvent {
    public long FromUserId { get; private set; }
    public long SquadId { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        Task.FromResult(SquadRegistry.Requests.Remove(FromUserId, connection.UserContainer.Id));
}
