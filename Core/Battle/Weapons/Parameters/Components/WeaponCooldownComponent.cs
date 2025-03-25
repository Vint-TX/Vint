using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(7115193786389139467)]
public class WeaponCooldownComponent : IComponent {
    public float CooldownIntervalSec { get; set; }
}
