using System.Collections.Frozen;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507799982015)]
public class AcceptRequestToSquadEvent(
    GameServer server
) : IServerEvent {
    public long FromUserId { get; private set; }
    public long SquadId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!SquadUtils.CanJoinSquad(connection) || !connection.InSquad) return;

        Squad squad = connection.Squad;
        if (squad.Entity.Id != SquadId || !squad.CanAddMember) return;

        IPlayerConnection? fromConnection = server.FindConnection(FromUserId);

        if (fromConnection == null || fromConnection.InSquad || !SquadUtils.CanJoinSquad(fromConnection))
            return;

        long targetId = connection.UserContainer.Id;
        if (!SquadRegistry.Requests.Remove(FromUserId, targetId))
            return;

        await squad.AddMember(fromConnection);

        if (!squad.HasSpace && SquadRegistry.Invites.RemoveAll(targetId, out FrozenSet<long>? targets))
            foreach (IPlayerConnection target in server.FindConnections(targets))
                await target.Send<InviteToSquadCanceledEvent>(target.UserContainer.Entity);
    }
}
