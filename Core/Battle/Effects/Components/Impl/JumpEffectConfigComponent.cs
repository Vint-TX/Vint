using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl;

[ProtocolId(1538548472363)]
public class JumpEffectConfigComponent(
    float forceUpgradeMult
) : IComponent {
    public float ForceUpgradeMult { get; private set; } = forceUpgradeMult;
}
