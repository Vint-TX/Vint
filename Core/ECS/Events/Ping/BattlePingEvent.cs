using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(1480326022618)]
public class BattlePingEvent : IServerEvent {
    public float ClientSendRealTime { get; private set; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) =>
        await connection.Send(new BattlePongEvent(ClientSendRealTime), entities.Single());
}
