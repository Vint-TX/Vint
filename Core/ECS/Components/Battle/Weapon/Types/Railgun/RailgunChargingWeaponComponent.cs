using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Railgun;

[ProtocolId(2654416098660377118)]
public class RailgunChargingWeaponComponent(
    float chargingTime
) : IComponent {
    public float ChargingTime { get; private set; } = chargingTime;
}