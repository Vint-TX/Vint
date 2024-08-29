namespace Vint.Core.ECS.Components.Server.Common;

public abstract class RangedComponent : IComponent {
    public float InitialValue { get; protected set; }
    public float FinalValue { get; protected set; }
}
