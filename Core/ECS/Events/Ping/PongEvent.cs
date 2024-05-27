using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(1115422024552825915)]
public class PongEvent : IServerEvent {
    public float PongCommandClientRealTime { get; private set; }
    public sbyte CommandId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        DateTimeOffset receiveTime = DateTimeOffset.UtcNow;
        float ping = receiveTime.ToUnixTimeMilliseconds() - PongCommandClientRealTime;

        await connection.Send(new PingResultEvent(receiveTime, ping));
        connection.PongReceiveTime = receiveTime;
    }
}
