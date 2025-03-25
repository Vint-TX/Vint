using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Effects.Components.Config.Healing;

public class HealingComponent : IComponent {
    public float Percent { get; private set; }
    public float HpPerMs { get; private set; }
}
