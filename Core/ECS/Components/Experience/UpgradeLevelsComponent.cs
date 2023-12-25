using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Experience;

[ProtocolId(1476865927439)]
public class UpgradeLevelsComponent : IComponent {
    public int[] LevelsExperiences { get; private set; } = null!;
}