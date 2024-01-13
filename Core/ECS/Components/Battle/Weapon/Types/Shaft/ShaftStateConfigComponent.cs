using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(635950079224407790)]
public class ShaftStateConfigComponent(
    float waitingToActivationTransitionTimeSec,
    float activationToWorkingTransitionTimeSec,
    float finishToIdleTransitionTimeSec
) : IComponent {
    public float WaitingToActivationTransitionTimeSec { get; set; } = waitingToActivationTransitionTimeSec;
    public float ActivationToWorkingTransitionTimeSec { get; set; } = activationToWorkingTransitionTimeSec;
    public float FinishToIdleTransitionTimeSec { get; set; } = finishToIdleTransitionTimeSec;
}