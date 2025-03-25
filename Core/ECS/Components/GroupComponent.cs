namespace Vint.Core.ECS.Components.Group;

public abstract class GroupComponent(
    long key
) : IComponent {
    public long Key { get; private set; } = key;
}
