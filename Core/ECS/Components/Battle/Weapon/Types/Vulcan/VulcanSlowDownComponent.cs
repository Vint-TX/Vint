using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

[ProtocolId(-6843896944033144903), ClientAddable, ClientRemovable]
public class VulcanSlowDownComponent : IComponent {
    public bool IsAfterShooting { get; private set; }
    public int ClientTime { get; private set; }
}