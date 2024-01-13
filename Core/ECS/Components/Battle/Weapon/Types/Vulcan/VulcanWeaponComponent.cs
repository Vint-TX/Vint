using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

[ProtocolId(4207390770640273134)]
public class VulcanWeaponComponent(
    float speedUpTime,
    float slowDownTime,
    float temperatureIncreasePerSec,
    float temperatureLimit,
    float temperatureHittingTime,
    float weaponTurnDecelerationCoeff,
    float targetHeatingMult
) : IComponent {
    public float SpeedUpTime { get; set; } = speedUpTime;
    public float SlowDownTime { get; set; } = slowDownTime;
    public float TemperatureIncreasePerSec { get; set; } = temperatureIncreasePerSec;
    public float TemperatureLimit { get; set; } = temperatureLimit;
    public float TemperatureHittingTime { get; set; } = temperatureHittingTime;
    public float WeaponTurnDecelerationCoeff { get; set; } = weaponTurnDecelerationCoeff;
    public float TargetHeatingMult { get; set; } = targetHeatingMult;
}