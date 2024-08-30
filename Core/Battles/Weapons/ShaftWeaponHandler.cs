using Vint.Core.Battles.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Components.Server.Weapon;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Weapons;

public class ShaftWeaponHandler : DiscreteTankWeaponHandler {
    public ShaftWeaponHandler(BattleTank battleTank) : base(battleTank) {
        EnergyDrainPerMs = ConfigManager.GetComponent<EnergyChargeSpeedPropertyComponent>(MarketConfigPath).FinalValue / 1000;
        AimingSpeedComponent = BattleEntity.GetComponent<ShaftAimingSpeedComponent>();
    }

    DateTimeOffset? AimingBeginTime { get; set; }
    public ShaftAimingSpeedComponent AimingSpeedComponent { get; }
    public bool Aiming { get; private set; }
    public TimeSpan AimingDuration { get; private set; }
    public float EnergyDrainPerMs { get; private set; }

    public override int MaxHitTargets => 1;

    public void Aim() {
        Aiming = true;
        AimingBeginTime = DateTimeOffset.UtcNow;
        BattleEntity.ChangeComponent<WeaponRotationComponent>(component => { // vertical speed controlled by client, but horizontal is not
            component.Speed = AimingSpeedComponent.MaxHorizontalSpeed;
            component.Acceleration = AimingSpeedComponent.HorizontalAcceleration;
        });
    }

    public void Idle() {
        double durationMs =
            Math.Clamp((DateTimeOffset.UtcNow - (AimingBeginTime ?? DateTimeOffset.UtcNow)).TotalMilliseconds, 0, 1 / EnergyDrainPerMs);

        AimingDuration = TimeSpan.FromMilliseconds(durationMs);
        BattleEntity.ChangeComponent(WeaponRotationComponent.Clone());
    }

    public void Reset() {
        Aiming = false;
        AimingBeginTime = null;
        AimingDuration = TimeSpan.Zero;
    }
}
