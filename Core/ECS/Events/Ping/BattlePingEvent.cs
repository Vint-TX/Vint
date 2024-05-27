using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(1480326022618)]
public class BattlePingEvent : IServerEvent {
    public float ClientSendRealTime { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        await connection.Send(new BattlePongEvent(ClientSendRealTime), entities.Single());
}
