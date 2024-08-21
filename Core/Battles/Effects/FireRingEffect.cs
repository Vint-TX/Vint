using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class FireRingEffect(
    TimeSpan cooldown,
    IEntity marketEntity,
    float radius,
    float minDamagePercent,
    float impact,
    float temperatureLimit,
    float temperatureDelta,
    float heatDamage,
    BattleTank tank,
    int level
) : Effect(tank, level), IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; private set; } = null!;

    float MinDamageRadius => radius;
    float MaxDamageRadius => 0;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new FireRingEffectTemplate().Create(Tank.BattlePlayer,
            Duration,
            Battle.Properties.FriendlyFire,
            impact,
            minDamagePercent,
            MinDamageRadius,
            MaxDamageRadius);

        WeaponHandler = new FireRingWeaponHandler(Tank, cooldown, marketEntity, Entity, temperatureLimit, temperatureDelta, heatDamage,
            minDamagePercent, MinDamageRadius, MaxDamageRadius);

        await ShareAll();
        Schedule(TimeSpan.FromSeconds(10), Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareAll();
        Entity = null;
    }
}
