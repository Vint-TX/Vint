using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Squads;

namespace Vint.Core.ECS.Events.Squad;

[ProtocolId(1510641456884)]
public class RejectRequestToSquadEvent : IServerEvent {
    public long FromUserId { get; private set; }
    public long SquadId { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        Task.FromResult(SquadRegistry.Requests.Remove(FromUserId, connection.UserContainer.Id));
}
