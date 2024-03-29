using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(6541712051864507498), ClientAddable, ClientRemovable]
public class ShaftWaitingStateComponent : IComponent {
    public float WaitingTimer { get; private set; }
    public int Time { get; private set; }
}