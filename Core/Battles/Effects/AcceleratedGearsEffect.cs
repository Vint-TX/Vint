using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class AcceleratedGearsEffect(
    BattleTank tank,
    int level,
    float turretSpeed,
    float turretAcceleration,
    float hullRotation
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);
        CanBeDeactivated = false;

        Tank.OriginalSpeedComponent.TurnSpeed *= hullRotation;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Speed *= turretSpeed;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Acceleration *= turretAcceleration;

        if (Tank.WeaponHandler is ShaftWeaponHandler shaft) {
            shaft.AimingSpeedComponent.MaxHorizontalSpeed *= turretSpeed;
            shaft.AimingSpeedComponent.HorizontalAcceleration *= turretAcceleration;
        }

        await Tank.UpdateSpeed();

        Entities.Add(new AcceleratedGearsEffectTemplate().Create(Tank.BattlePlayer, Duration));
        await ShareAll();
    }

    public override async Task Deactivate() {
        if (!CanBeDeactivated || !IsActive) return;

        Tank.Effects.TryRemove(this);

        Tank.OriginalSpeedComponent.TurnSpeed /= hullRotation;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Speed /= turretSpeed;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Acceleration /= turretAcceleration;

        if (Tank.WeaponHandler is ShaftWeaponHandler shaft) {
            shaft.AimingSpeedComponent.MaxHorizontalSpeed /= turretSpeed;
            shaft.AimingSpeedComponent.HorizontalAcceleration /= turretAcceleration;
        }

        await Tank.UpdateSpeed();
        await UnshareAll();

        Entities.Clear();
    }
}
