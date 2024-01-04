using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496905821016)]
public class SetEquipmentEvent : IServerEvent {
    public long WeaponId { get; private set; }
    public long HullId { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity lobby = entities.Single();
    }
}