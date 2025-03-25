using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class ShaftStateConfig : RangedComponent, IConvertible<ShaftStateConfigComponent> {
    public void Convert(ShaftStateConfigComponent component) {
        component.WaitingToActivationTransitionTimeSec = FinalValue;
        component.ActivationToWorkingTransitionTimeSec = FinalValue;
        component.FinishToIdleTransitionTimeSec = FinalValue;
    }
}
