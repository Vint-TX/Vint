using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(1538548472363)]
public class JumpEffectConfigComponent(
    float forceUpgradeMult
) : IComponent {
    public float ForceUpgradeMult { get; private set; } = forceUpgradeMult;
}
