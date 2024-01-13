using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

[ProtocolId(-6843896944033144903)]
public class VulcanSlowDownComponent : IComponent { // todo
    public bool IsAfterShooting { get; private set; }
    public int ClientTime { get; private set; }
}