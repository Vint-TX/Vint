using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

[ProtocolId(-5670596162316552032), ClientAddable, ClientRemovable]
public class ShaftAimingWorkFinishStateComponent : IComponent {
    public float FinishTimer { get; private set; }
    public int ClientTime { get; private set; }
}
