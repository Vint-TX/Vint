using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components;

[ProtocolId(1542363613520)]
public class SplashEffectComponent(
    bool canTargetTeammates
) : IComponent {
    public bool CanTargetTeammates { get; } = canTargetTeammates;
}
