using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class HealthComponent : RangedComponent, IConvertible<Components.HealthComponent>, IConvertible<HealthConfigComponent> {
    public void Convert(Components.HealthComponent component) {
        component.CurrentHealth = InitialValue;
        component.MaxHealth = FinalValue;
    }

    public void Convert(HealthConfigComponent component) {
        component.BaseHealth = InitialValue;
    }
}
