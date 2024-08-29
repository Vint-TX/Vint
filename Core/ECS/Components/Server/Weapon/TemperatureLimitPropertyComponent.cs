using Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class TemperatureLimitPropertyComponent : RangedComponent, IConvertible<VulcanWeaponComponent> {
    public void Convert(VulcanWeaponComponent component) =>
        component.TemperatureLimit = FinalValue;
}
