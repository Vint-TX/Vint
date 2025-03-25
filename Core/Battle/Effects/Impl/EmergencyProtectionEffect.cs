using Vint.Core.Battle.Effects.Templates;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons.Handlers;
using Vint.Core.Battle.Weapons.Parameters.Components;

namespace Vint.Core.Battle.Effects.Impl;

public class EmergencyProtectionEffect : Effect, IDamageMultiplierEffect {
    public EmergencyProtectionEffect(TimeSpan duration, BattleTank tank, int level) : base(tank, level) =>
        Duration = duration;

    public float Multiplier => 1;

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) => 0;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);
        await Tank.Entities.Weapon.RemoveComponentIfPresent<ShootableComponent>();
        await ResetWeaponState();

        Entity = new EmergencyProtectionEffectTemplate().Create(Tank.Tanker, Duration);
        await ShareToAllPlayers();

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);
        await Tank.Entities.Weapon.AddComponentIfAbsent<ShootableComponent>();

        await UnshareFromAllPlayers();
        Entity = null;
    }

    Task ResetWeaponState() =>
        Tank.WeaponHandler is StreamWeaponHandler streamWeapon
            ? streamWeapon.Reset()
            : Task.CompletedTask;
}
