using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1496052424091)]
public class BattleLobbyGroupComponent(
    long key
) : GroupComponent(key) {
    public BattleLobbyGroupComponent(IEntity entity) : this(entity.Id) { }
}