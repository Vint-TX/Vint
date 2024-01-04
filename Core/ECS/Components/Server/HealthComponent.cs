using Vint.Core.ECS.Components.Battle.Parameters.Health;

namespace Vint.Core.ECS.Components.Server;

public class HealthComponent : RangedComponent, IConvertible<Battle.Parameters.Health.HealthComponent>, IConvertible<HealthConfigComponent> {
    public void Convert(Battle.Parameters.Health.HealthComponent component) {
        component.CurrentHealth = InitialValue;
        component.MaxHealth = FinalValue;
    }

    public void Convert(HealthConfigComponent component) {
        component.BaseHealth = InitialValue;
    }
}