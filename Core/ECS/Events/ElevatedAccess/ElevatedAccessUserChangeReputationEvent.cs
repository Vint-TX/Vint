using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1522660970570)]
public class ElevatedAccessUserChangeReputationEvent : IServerEvent {
    public int Count { get; private set; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin) return;

        await connection.ChangeReputation(Count);
    }
}
