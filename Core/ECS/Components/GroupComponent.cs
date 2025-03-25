namespace Vint.Core.ECS.Components;

public abstract class GroupComponent(
    long key
) : IComponent {
    public long Key { get; private set; } = key;
}
