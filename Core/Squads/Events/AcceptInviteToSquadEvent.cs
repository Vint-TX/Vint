using System.Collections.Frozen;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507538648077)]
public class AcceptInviteToSquadEvent(
    GameServer server
) : IServerEvent {
    public long FromUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!SquadUtils.CanJoinSquad(connection)) return;

        long targetId = connection.UserContainer.Id;
        IPlayerConnection? fromConnection = server.FindConnection(FromUserId);

        if (fromConnection == null || !SquadRegistry.Invites.Remove(FromUserId, targetId))
            return;

        Squads.Squad? squad = fromConnection.Squad;

        if (squad == null) {
            if (!SquadUtils.CanJoinSquad(fromConnection)) return;

            squad = new Squads.Squad();
            await squad.AddMember(fromConnection);
            await squad.SetLeader(fromConnection.UserContainer.Id);
        }

        await squad.AddMember(connection);

        if (!squad.HasSpace && SquadRegistry.Invites.RemoveAll(FromUserId, out FrozenSet<long>? targets)) {
            foreach (IPlayerConnection target in server.FindConnections(targets))
                await target.Send<InviteToSquadCanceledEvent>(target.UserContainer.Entity);
        }
    }
}
