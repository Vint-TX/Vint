using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.Battle.Weapons.Components.Stream;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class EnergyChargeSpeedPropertyComponent : RangedComponent, IConvertible<StreamWeaponEnergyComponent>, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.UnloadAimingEnergyPerSec = FinalValue;

    public void Convert(StreamWeaponEnergyComponent component) =>
        component.UnloadEnergyPerSec = FinalValue;
}
