using Vint.Core.Battle.Weapons.Components.Vulcan;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class WeaponTurnDecelerationPropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) => component.WeaponTurnDecelerationCoeff = FinalValue;
}
