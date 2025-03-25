using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Effects.Components.Config.Common;

public abstract class FactorComponent : IComponent {
    public float Factor { get; protected set; }
}
