using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Squad;

[ProtocolId(1507727447201)]
public class ChangeSquadLeaderEvent : IServerEvent {
    public long NewLeaderUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InSquad || connection.Squad.Leader != connection) return;

        await connection.Squad.SetLeader(NewLeaderUserId);
    }
}
