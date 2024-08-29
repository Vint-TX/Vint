using Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class TargetHeatMultiplierPropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.TargetHeatingMult = FinalValue;
}
