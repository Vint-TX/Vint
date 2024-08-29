using Vint.Core.ECS.Components.Battle.Parameters.Health;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Tank;

public class HealthComponent : RangedComponent, IConvertible<Components.Battle.Parameters.Health.HealthComponent>, IConvertible<HealthConfigComponent> {
    public void Convert(Components.Battle.Parameters.Health.HealthComponent component) {
        component.CurrentHealth = InitialValue;
        component.MaxHealth = FinalValue;
    }

    public void Convert(HealthConfigComponent component) {
        component.BaseHealth = InitialValue;
    }
}
