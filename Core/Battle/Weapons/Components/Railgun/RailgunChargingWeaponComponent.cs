using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Railgun;

[ProtocolId(2654416098660377118)]
public class RailgunChargingWeaponComponent : IComponent {
    public float ChargingTime { get; set; }
}
