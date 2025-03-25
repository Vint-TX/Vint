using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Effects.Components.Config.Healing;

public class TickComponent : IComponent {
    public float Period { get; private set; }
}
