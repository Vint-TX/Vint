using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(1480333679186)]
public class BattlePingResultEvent : IServerEvent { // todo ??
    public float ClientSendRealTime { get; private set; }
    public float ClientReceiveRealTime { get; private set; }

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) => Task.CompletedTask;
}
