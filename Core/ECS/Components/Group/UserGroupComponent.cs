using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(7453043498913563889)]
public class UserGroupComponent(
    long key
) : GroupComponent(key) {
    public UserGroupComponent(IEntity entity) : this(entity.Id) { }
}