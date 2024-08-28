using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(635950079224407790)]
public class ShaftStateConfigComponent : IComponent {
    public float WaitingToActivationTransitionTimeSec { get; set; }
    public float ActivationToWorkingTransitionTimeSec { get; set; }
    public float FinishToIdleTransitionTimeSec { get; set; }
}
