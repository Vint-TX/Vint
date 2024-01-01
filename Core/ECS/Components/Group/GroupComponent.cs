using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Components.Group;

public abstract class GroupComponent(
    long key
) : IComponent {
    protected GroupComponent(IEntity entity) : this(entity.Id) { }

    public long Key { get; private set; } = key;
}