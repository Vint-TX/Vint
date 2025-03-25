using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Effects.Components.Config.Common;

public abstract class ModuleEffectUpgradablePropertyComponent : IComponent {
    public bool LinearInterpolation { get; protected set; }
    public List<float> UpgradeLevel2Values { get; protected set; } = null!;
}
