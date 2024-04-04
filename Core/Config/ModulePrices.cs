using Vint.Core.ECS.Components.Modules;

namespace Vint.Core.Config;

public readonly record struct ModulePrices(
    List<ModulePrice> FirstTier,
    List<ModulePrice> SecondTier,
    List<ModulePrice> ThirdTier
);