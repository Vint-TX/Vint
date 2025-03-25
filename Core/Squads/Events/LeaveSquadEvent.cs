using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507722104935)]
public class LeaveSquadEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InSquad) return;

        await connection.Squad.RemoveMember(connection.UserContainer.Id);
    }
}
