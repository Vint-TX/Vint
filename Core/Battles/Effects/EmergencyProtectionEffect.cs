using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class EmergencyProtectionEffect : Effect, IDamageMultiplierEffect {
    public EmergencyProtectionEffect(TimeSpan duration, BattleTank tank, int level) : base(tank, level) =>
        Duration = duration;

    public float Multiplier => 1;

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);
        await Tank.Weapon.RemoveComponentIfPresent<ShootableComponent>();
        await ResetWeaponState();

        Entity = new EmergencyProtectionEffectTemplate().Create(Tank.BattlePlayer, Duration);
        await ShareToAllPlayers();

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);
        await Tank.Weapon.AddComponentIfAbsent<ShootableComponent>();

        await UnshareFromAllPlayers();
        Entity = null;
    }

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) => 0;

    Task ResetWeaponState() =>
        Tank.WeaponHandler is StreamWeaponHandler streamWeapon
            ? streamWeapon.Reset()
            : Task.CompletedTask;
}
