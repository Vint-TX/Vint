using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class AcceleratedGearsEffect(
    BattleTank tank,
    int level,
    float turretSpeed,
    float turretAcceleration,
    float hullRotation
) : Effect(tank, level) {
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        CanBeDeactivated = false;
        
        Tank.OriginalSpeedComponent.TurnSpeed *= hullRotation;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Speed *= turretSpeed;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Acceleration *= turretAcceleration;
        
        Tank.UpdateSpeed();
        
        Entities.Add(new AcceleratedGearsEffectTemplate().Create(Tank.BattlePlayer, Duration));
        ShareAll();
    }

    public override void Deactivate() {
        if (!CanBeDeactivated || !IsActive) return;
        
        Tank.Effects.TryRemove(this);
        
        Tank.OriginalSpeedComponent.TurnSpeed /= hullRotation;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Speed /= turretSpeed;
        Tank.WeaponHandler.OriginalWeaponRotationComponent.Acceleration /= turretAcceleration;
        
        Tank.UpdateSpeed();
        UnshareAll();
        
        Entities.Clear();
    }
}