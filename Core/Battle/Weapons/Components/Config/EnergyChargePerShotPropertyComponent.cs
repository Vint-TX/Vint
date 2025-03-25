using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class EnergyChargePerShotPropertyComponent : RangedComponent, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.UnloadEnergyPerQuickShot = FinalValue;
}
