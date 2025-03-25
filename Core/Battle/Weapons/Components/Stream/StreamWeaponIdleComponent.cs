using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Stream;

[ProtocolId(1498352458940656986), ClientAddable, ClientRemovable]
public class StreamWeaponIdleComponent : IComponent {
    public int Time { get; private set; }
}
