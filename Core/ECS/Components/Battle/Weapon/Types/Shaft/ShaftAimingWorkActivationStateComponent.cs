using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(8631717637564140236), ClientAddable, ClientRemovable]
public class ShaftAimingWorkActivationStateComponent : IComponent {
    public float ActivationTimer { get; private set; }
    public int ClientTime { get; private set; }
}