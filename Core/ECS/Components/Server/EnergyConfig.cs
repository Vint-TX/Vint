using Vint.Core.ECS.Components.Battle.Weapon.Stream;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

namespace Vint.Core.ECS.Components.Server;

public class EnergyChargePerShotPropertyComponent : RangedComponent, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.UnloadEnergyPerQuickShot = FinalValue;
}

public class EnergyChargeSpeedPropertyComponent : RangedComponent, IConvertible<StreamWeaponEnergyComponent>, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.UnloadAimingEnergyPerSec = FinalValue;

    public void Convert(StreamWeaponEnergyComponent component) =>
        component.UnloadEnergyPerSec = FinalValue;
}

public class EnergyRechargeSpeedPropertyComponent : RangedComponent, IConvertible<StreamWeaponEnergyComponent>, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.ReloadEnergyPerSec = FinalValue;

    public void Convert(StreamWeaponEnergyComponent component) =>
        component.ReloadEnergyPerSec = FinalValue;
}