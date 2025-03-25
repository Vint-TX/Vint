using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(7453043498913563889)]
public class UserGroupComponent(
    long key
) : GroupComponent(key) {
    public UserGroupComponent(IEntity entity) : this(entity.Id) { }
}
