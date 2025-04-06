using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Simulations.Components;

public class ColorConfigComponent : IComponent {
    public Dictionary<string, string> Colors { get; private set; } = null!;
}
