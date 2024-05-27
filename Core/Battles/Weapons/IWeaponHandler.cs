using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public interface IWeaponHandler {
    public IDamageCalculator DamageCalculator { get; }
    public BattleTank BattleTank { get; }

    public TimeSpan Cooldown { get; }

    public bool DamageWeakeningByDistance { get; }
    public float MaxDamageDistance { get; }
    public float MinDamageDistance { get; }
    public float MinDamagePercent { get; }

    public DateTimeOffset LastHitTime { get; set; }
    public int MaxHitTargets { get; }

    public Task Fire(HitTarget target, int targetIndex);

    public Task OnTankEnable();

    public Task OnTankDisable();

    public Task Tick();
}
