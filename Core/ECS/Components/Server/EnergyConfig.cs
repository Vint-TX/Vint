using Vint.Core.ECS.Components.Battle.Weapon.Stream;

namespace Vint.Core.ECS.Components.Server;

public class EnergyConfig {
    public class EnergyChargePerShotPropertyComponent : RangedComponent;

    public class EnergyChargeSpeedPropertyComponent : RangedComponent, IConvertible<StreamWeaponEnergyComponent> {
        public void Convert(StreamWeaponEnergyComponent component) =>
            component.UnloadEnergyPerSec = FinalValue;
    }

    public class EnergyRechargeSpeedPropertyComponent : RangedComponent, IConvertible<StreamWeaponEnergyComponent> {
        public void Convert(StreamWeaponEnergyComponent component) =>
            component.ReloadEnergyPerSec = FinalValue;
    }
}