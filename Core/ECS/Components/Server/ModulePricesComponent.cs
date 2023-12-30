using Vint.Core.ECS.Components.Modules;

namespace Vint.Core.ECS.Components.Server;

public class ModulePricesComponent : IComponent {
    public List<ModulePrice> FirstTier { get; private set; } = null!;
    public List<ModulePrice> SecondTier { get; private set; } = null!;
    public List<ModulePrice> ThirdTier { get; private set; } = null!;
}