using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class EnergyChargePerShotPropertyComponent : RangedComponent, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.UnloadEnergyPerQuickShot = FinalValue;
}
