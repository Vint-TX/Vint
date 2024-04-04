using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Inventory;

[ProtocolId(636378740801778877)]
public class InventoryLimitBundleEffectsComponent(
    int bundleEffectLimit
) : IComponent {
    public int BundleEffectLimit { get; } = bundleEffectLimit;
}