using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

[ProtocolId(-3791262141248621103)]
public class VulcanIdleComponent : IComponent {
    public int Time { get; private set; }
}