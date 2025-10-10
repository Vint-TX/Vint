using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507211574274)]
public class InviteToSquadEvent(
    GameServer server
) : IServerEvent {
    public long InvitedUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long sourceId = connection.UserContainer.Id;

        if (sourceId == InvitedUserId ||
            !connection.IsLoggedIn ||
            (connection.InSquad && !connection.Squad.CanAddMember) ||
            !SquadUtils.CanJoinSquad(connection)) return;

        IPlayerConnection? targetConnection = server.FindConnection(InvitedUserId);
        if (targetConnection == null || targetConnection.InSquad || !SquadUtils.CanJoinSquad(targetConnection)) return;

        if (!SquadRegistry.Invites.Add(sourceId, InvitedUserId))
            return;

        await targetConnection.Send(new InvitedToSquadEvent(connection.Player.Username, sourceId), targetConnection.UserContainer.Entity);
    }
}
