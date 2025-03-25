using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class MountUpgradeLevelRestrictionComponent : IComponent {
    public int RestrictionValue { get; private set; }
}
