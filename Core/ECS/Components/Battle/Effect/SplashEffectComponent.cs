using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect;

[ProtocolId(1542363613520)]
public class SplashEffectComponent(
    bool canTargetTeammates
) : IComponent {
    public bool CanTargetTeammates { get; } = canTargetTeammates;
}