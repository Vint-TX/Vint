using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battle.Effects;

public class KamikadzeEffect(
    TimeSpan cooldown,
    IEntity marketEntity,
    float radius,
    float minPercent,
    float maxDamage,
    float minDamage,
    float impact,
    BattleTank tank,
    int level
) : WeaponEffect(tank, level) {
    public override ModuleWeaponHandler WeaponHandler { get; protected set; } = null!;

    public override async Task Activate() {
        if (IsActive) return;

        CanBeDeactivated = false;
        Tank.Effects.Add(this);

        WeaponEntity = Entity = new KamikadzeEffectTemplate().Create(Tank.Tanker,
            Duration,
            Round.Properties.FriendlyFire,
            impact,
            minPercent,
            0,
            radius);

        WeaponHandler = new KamikadzeWeaponHandler(Tank,
            Round.DamageCalculator,
            cooldown,
            marketEntity,
            Entity,
            true,
            0,
            radius,
            minPercent,
            maxDamage,
            minDamage,
            int.MaxValue);

        await ShareToAllPlayers();
        Schedule(TimeSpan.FromSeconds(10), DeactivateInternal);
    }

    public override async Task Deactivate() {
        if (!IsActive || !CanBeDeactivated)
            return;

        Tank.Effects.TryRemove(this);
        await UnshareFromAllPlayers();

        Entity = null;
    }

    async Task DeactivateInternal() {
        CanBeDeactivated = true;
        await Deactivate();
    }

    public override async Task DeactivateByEMP() =>
        await DeactivateInternal();
}
