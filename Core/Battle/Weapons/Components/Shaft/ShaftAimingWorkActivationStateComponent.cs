using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

[ProtocolId(8631717637564140236), ClientAddable, ClientRemovable]
public class ShaftAimingWorkActivationStateComponent : IComponent {
    public float ActivationTimer { get; private set; }
    public int ClientTime { get; private set; }
}
