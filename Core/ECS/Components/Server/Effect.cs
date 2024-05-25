namespace Vint.Core.ECS.Components.Server;

public abstract class FactorComponent : IComponent {
    public float Factor { get; protected set; }
}

public class DurationComponent : IComponent {
    public float Duration { get; private set; }
}

public class TickComponent : IComponent {
    public float Period { get; private set; }
}

public class HealingComponent : IComponent {
    public float Percent { get; private set; }
    public float HpPerMs { get; private set; }
}

public class ArmorEffectComponent : FactorComponent;

public class DamageEffectComponent : FactorComponent;

public class TurboSpeedEffectComponent : IComponent {
    public float SpeedCoeff { get; private set; }
}
