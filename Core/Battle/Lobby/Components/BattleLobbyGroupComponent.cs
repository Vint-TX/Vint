using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Components;

[ProtocolId(1496052424091)]
public class BattleLobbyGroupComponent(
    long key
) : GroupComponent(key) {
    public BattleLobbyGroupComponent(IEntity entity) : this(entity.Id) { }
}
