using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Vulcan;

[ProtocolId(4207390770640273134)]
public class VulcanWeaponComponent : IComponent {
    public float SpeedUpTime { get; set; }
    public float SlowDownTime { get; set; }
    public float TemperatureIncreasePerSec { get; set; }
    public float TemperatureLimit { get; set; }
    public float TemperatureHittingTime { get; set; }
    public float WeaponTurnDecelerationCoeff { get; set; }
    public float TargetHeatingMult { get; set; }
}
