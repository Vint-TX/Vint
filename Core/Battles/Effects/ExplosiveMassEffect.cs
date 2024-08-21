using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class ExplosiveMassEffect(
    TimeSpan cooldown,
    IEntity marketEntity,
    float radius,
    float delay,
    float maxDamage,
    float minDamage,
    BattleTank tank,
    int level
) : Effect(tank, level), IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; private set; } = null!;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new ExplosiveMassEffectTemplate().Create(Tank.BattlePlayer, Duration, radius, delay);
        ExplosiveMassWeaponHandler weaponHandler = new(Tank, cooldown, marketEntity, Entity, maxDamage, minDamage);

        weaponHandler.Exploded += Deactivate;
        WeaponHandler = weaponHandler;

        await ShareAll();
        Schedule(TimeSpan.FromMilliseconds(delay) + TimeSpan.FromSeconds(10), Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareAll();
        Entity = null;
    }
}
