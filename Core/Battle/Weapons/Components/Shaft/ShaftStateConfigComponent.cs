using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

[ProtocolId(635950079224407790)]
public class ShaftStateConfigComponent : IComponent {
    public float WaitingToActivationTransitionTimeSec { get; set; }
    public float ActivationToWorkingTransitionTimeSec { get; set; }
    public float FinishToIdleTransitionTimeSec { get; set; }
}
