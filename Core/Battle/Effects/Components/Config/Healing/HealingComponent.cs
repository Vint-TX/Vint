namespace Vint.Core.ECS.Components.Server.Modules.Effect.Healing;

public class HealingComponent : IComponent {
    public float Percent { get; private set; }
    public float HpPerMs { get; private set; }
}
