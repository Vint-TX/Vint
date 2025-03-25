using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons.Components.Config;
using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.Config;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Weapons.Handlers.Impl;

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

    public async Task Aim() {
        Aiming = true;
        AimingBeginTime = DateTimeOffset.UtcNow;

        await BattleEntity.ChangeComponent<WeaponRotationComponent>(component => { // vertical speed controlled by client, but horizontal is not
            component.Speed = AimingSpeedComponent.MaxHorizontalSpeed;
            component.Acceleration = AimingSpeedComponent.HorizontalAcceleration;
        });
    }

    public async Task Idle() {
        double durationMs = Math.Clamp((DateTimeOffset.UtcNow - (AimingBeginTime ?? DateTimeOffset.UtcNow)).TotalMilliseconds,
            0,
            1 / EnergyDrainPerMs);

        AimingDuration = TimeSpan.FromMilliseconds(durationMs);
        await BattleEntity.ChangeComponent(WeaponRotationComponent.Clone());
    }

    public void Reset() {
        Aiming = false;
        AimingBeginTime = null;
        AimingDuration = TimeSpan.Zero;
    }
}
