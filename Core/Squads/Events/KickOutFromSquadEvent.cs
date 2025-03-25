using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507210730593)]
public class KickOutFromSquadEvent : IServerEvent {
    public long KickedOutUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InSquad || connection.Squad.Leader != connection) return;

        await connection.Squad.RemoveMember(KickedOutUserId);
    }
}
