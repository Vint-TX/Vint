using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class ShaftStateConfig : RangedComponent, IConvertible<ShaftStateConfigComponent> {
    public void Convert(ShaftStateConfigComponent component) {
        component.WaitingToActivationTransitionTimeSec = FinalValue;
        component.ActivationToWorkingTransitionTimeSec = FinalValue;
        component.FinishToIdleTransitionTimeSec = FinalValue;
    }
}
