using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(636319924231605265)]
public class ModuleCardsCompositionComponent : IComponent {
    public ModuleCardsCompositionComponent(int tier) {
        ModulePricesComponent modulePricesComponent = ConfigManager.GetComponent<ModulePricesComponent>("garage/module/module");

        List<ModulePrice> modulePrices = tier switch {
            0 => modulePricesComponent.FirstTier,
            1 => modulePricesComponent.SecondTier,
            2 => modulePricesComponent.ThirdTier,
            _ => throw new NotSupportedException()
        };

        CraftPrice = modulePrices.First();
        UpgradePrices = modulePrices.Skip(1).ToList();
    }

    public ModulePrice CraftPrice { get; private set; }
    public List<ModulePrice> UpgradePrices { get; private set; }
}

public class ModulePrice {
    public int Cards { get; private set; }
    public int Crystals { get; private set; }
    public int XCrystals { get; private set; }
}