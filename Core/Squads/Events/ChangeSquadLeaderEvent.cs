using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507727447201)]
public class ChangeSquadLeaderEvent : IServerEvent {
    public long NewLeaderUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InSquad || connection.Squad.Leader != connection) return;

        await connection.Squad.SetLeader(NewLeaderUserId);
    }
}
