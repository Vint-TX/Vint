using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507792868618)]
public class RequestToSquadEvent(
    GameServer server
) : IServerEvent {
    public long ToUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long sourceId = connection.UserContainer.Id;
        if (sourceId == ToUserId || connection.InSquad || !SquadUtils.CanJoinSquad(connection)) return;

        IPlayerConnection? toConnection = server.FindConnection(ToUserId);
        if (toConnection is not { InSquad: true } || !SquadUtils.CanJoinSquad(toConnection))
            return;

        Squad squad = toConnection.Squad;

        if (!squad.CanAddMember) {
            await connection.Send(new RequestToSquadRejectedEvent(RejectRequestToSquadReason.SquadIsFull, ToUserId));
            return;
        }

        if (!SquadRegistry.Requests.Add(sourceId, ToUserId))
            return;

        await toConnection.Send(new RequestedToSquadEvent(connection.Player.Username, sourceId, squad.Entity.Id), toConnection.UserContainer.Entity);
    }
}
