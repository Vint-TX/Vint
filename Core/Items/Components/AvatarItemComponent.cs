using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class AvatarItemComponent : IComponent {
    public string Id { get; private set; } = null!;
}
