using Vint.Core.Battle.Modules.Common.Components;

namespace Vint.Core.Config;

public readonly record struct ModulePrices(
    List<ModulePrice> FirstTier,
    List<ModulePrice> SecondTier,
    List<ModulePrice> ThirdTier
);
