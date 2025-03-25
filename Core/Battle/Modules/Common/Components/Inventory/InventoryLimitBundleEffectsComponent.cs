using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components.Inventory;

[ProtocolId(636378740801778877)]
public class InventoryLimitBundleEffectsComponent(
    int bundleEffectLimit
) : IComponent {
    public int BundleEffectLimit { get; } = bundleEffectLimit;
}
