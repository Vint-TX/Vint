using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(-5670596162316552032), ClientAddable, ClientRemovable]
public class ShaftAimingWorkFinishStateComponent : IComponent {
    public float FinishTimer { get; private set; }
    public int ClientTime { get; private set; }
}