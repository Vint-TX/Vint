using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1522660970570)]
public class ElevatedAccessUserChangeReputationEvent : IServerEvent {
    public int Count { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin || Count < 0) return;

        connection.ChangeReputation((uint)Count);
    }
}