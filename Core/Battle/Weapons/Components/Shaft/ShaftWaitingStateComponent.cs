using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

[ProtocolId(6541712051864507498), ClientAddable, ClientRemovable]
public class ShaftWaitingStateComponent : IComponent {
    public float WaitingTimer { get; private set; }
    public int Time { get; private set; }
}
