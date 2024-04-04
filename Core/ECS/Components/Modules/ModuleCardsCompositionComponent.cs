using Vint.Core.Config;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(636319924231605265)]
public class ModuleCardsCompositionComponent : IComponent {
    public ModuleCardsCompositionComponent(int tier) {
        ModulePrices modulePrices = ConfigManager.ModulePrices;

        List<ModulePrice> prices = tier switch {
            0 => modulePrices.FirstTier,
            1 => modulePrices.SecondTier,
            2 => modulePrices.ThirdTier,
            _ => throw new ArgumentException(null, nameof(tier))
        };

        CraftPrice = prices.First();
        UpgradePrices = prices.Skip(1).ToList();
    }

    public ModulePrice CraftPrice { get; private set; }
    public List<ModulePrice> UpgradePrices { get; private set; }
}

public class ModulePrice {
    public int Cards { get; set; }
    public int Crystals { get; set; }
    public int XCrystals { get; set; }
}