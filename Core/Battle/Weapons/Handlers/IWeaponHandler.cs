using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public interface IWeaponHandler {
    BattleTank BattleTank { get; }

    TimeSpan Cooldown { get; }

    bool DamageWeakeningByDistance { get; }
    float MaxDamageDistance { get; }
    float MinDamageDistance { get; }
    float MinDamagePercent { get; }

    int MaxHitTargets { get; }

    Task Fire(HitTarget target, int targetIndex);

    Task OnTankEnable();

    Task OnTankDisable();

    Task Tick(TimeSpan deltaTime);
}
