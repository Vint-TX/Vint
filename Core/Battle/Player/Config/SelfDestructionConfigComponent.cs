using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Player.Config;

public class SelfDestructionConfigComponent : IComponent {
    public int SuicideDurationTime { get; private set; }
}
