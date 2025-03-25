using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battle.Effects;

public class ExplosiveMassEffect(
    TimeSpan cooldown,
    IEntity marketEntity,
    float radius,
    float delay,
    float maxDamage,
    float minDamage,
    BattleTank tank,
    int level
) : WeaponEffect(tank, level) {
    public override ModuleWeaponHandler WeaponHandler { get; protected set; } = null!;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        WeaponEntity = Entity = new ExplosiveMassEffectTemplate().Create(Tank.Tanker, Duration, radius, delay);
        ExplosiveMassWeaponHandler weaponHandler = new(Tank, Round.DamageCalculator, cooldown, marketEntity, Entity, maxDamage, minDamage);

        weaponHandler.Exploded += Deactivate;
        WeaponHandler = weaponHandler;

        await ShareToAllPlayers();
        Schedule(TimeSpan.FromMilliseconds(delay) + TimeSpan.FromSeconds(10), Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }
}
