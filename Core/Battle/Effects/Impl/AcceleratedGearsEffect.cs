using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battle.Effects;

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

        Tank.SpeedComponent.TurnSpeed *= hullRotation;
        Tank.WeaponHandler.WeaponRotationComponent.Speed *= turretSpeed;
        Tank.WeaponHandler.WeaponRotationComponent.Acceleration *= turretAcceleration;

        if (Tank.WeaponHandler is ShaftWeaponHandler shaft) {
            shaft.AimingSpeedComponent.MaxHorizontalSpeed *= turretSpeed;
            shaft.AimingSpeedComponent.HorizontalAcceleration *= turretAcceleration;
        }

        await Tank.UpdateSpeed();

        Entity = new AcceleratedGearsEffectTemplate().Create(Tank.Tanker, Duration);
        await ShareToAllPlayers();
    }

    public override async Task Deactivate() {
        if (!CanBeDeactivated ||
            !IsActive) return;

        Tank.Effects.TryRemove(this);

        Tank.SpeedComponent.TurnSpeed /= hullRotation;
        Tank.WeaponHandler.WeaponRotationComponent.Speed /= turretSpeed;
        Tank.WeaponHandler.WeaponRotationComponent.Acceleration /= turretAcceleration;

        if (Tank.WeaponHandler is ShaftWeaponHandler shaft) {
            shaft.AimingSpeedComponent.MaxHorizontalSpeed /= turretSpeed;
            shaft.AimingSpeedComponent.HorizontalAcceleration /= turretAcceleration;
        }

        await Tank.UpdateSpeed();
        await UnshareFromAllPlayers();

        Entity = null;
    }

    public override async Task DeactivateByEMP() {
        CanBeDeactivated = true;
        await Deactivate();
    }
}
