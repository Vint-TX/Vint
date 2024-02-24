using Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

namespace Vint.Core.ECS.Components.Server;

public class SpinUpTimePropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.SpeedUpTime = FinalValue;
}

public class SpinDownTimePropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.SlowDownTime = FinalValue;
}

public class TemperatureHittingTimePropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.TemperatureHittingTime = FinalValue;
}

public class WeaponTurnDecelerationPropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.WeaponTurnDecelerationCoeff = FinalValue;
}

public class TargetHeatMultiplierPropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.TargetHeatingMult = FinalValue;
}