using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.Battles.Weapons;

public class ShaftWeaponHandler(
    BattleTank battleTank
) : DiscreteWeaponHandler(battleTank) {
    public DateTimeOffset? AimingBeginTime { get; set; }
    TimeSpan AimingDuration { get; set; }

    public void Aim() {
        AimingBeginTime = DateTimeOffset.UtcNow;
        BattleEntity.ChangeComponent<WeaponRotationComponent>(component => component.Speed *= 0.3f);
    }

    public void Idle() {
        AimingDuration = DateTimeOffset.UtcNow - (AimingBeginTime ?? DateTimeOffset.UtcNow);
        float energy = (float)Math.Clamp(0.9 - AimingDuration.TotalMilliseconds * 0.0002, 0, 1);
        
        BattleEntity.ChangeComponent<WeaponEnergyComponent>(component => component.Energy = energy);
        BattleEntity.ChangeComponent(((IComponent)OriginalWeaponRotationComponent).Clone());
    }

    public void Reset() {
        AimingBeginTime = null;
        AimingDuration = TimeSpan.Zero;
    }
}