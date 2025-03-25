namespace Vint.Core.ECS.Components.Server.Modules.Effect.Common;

public abstract class ModuleEffectUpgradablePropertyComponent : IComponent {
    public bool LinearInterpolation { get; protected set; }
    public List<float> UpgradeLevel2Values { get; protected set; } = null!;
}
