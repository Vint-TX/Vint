using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496905821016)]
public class SetEquipmentEvent : IServerEvent { // todo ??
    public long WeaponId { get; private set; }
    public long HullId { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) => Task.CompletedTask;
}
