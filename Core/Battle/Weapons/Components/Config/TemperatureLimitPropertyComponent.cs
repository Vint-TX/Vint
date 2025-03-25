using Vint.Core.Battle.Weapons.Components.Vulcan;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class TemperatureLimitPropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) =>
        component.TemperatureLimit = FinalValue;
}
