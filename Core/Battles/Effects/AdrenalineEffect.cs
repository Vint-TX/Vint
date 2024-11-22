using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;

namespace Vint.Core.Battles.Effects;

public class AdrenalineEffect(
    BattleTank tank,
    int level,
    float cooldownSpeedCoeff,
    float damageMultiplier
) : Effect(tank, level), IDamageMultiplierEffect {
    public float Multiplier { get; } = damageMultiplier;

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) =>
        source == Tank
            ? Multiplier
            : 1;

    public event Action? Deactivated;

    public override async Task Activate() {
        Tank.Effects.Add(this);

        await Tank.UpdateModuleCooldownSpeed(cooldownSpeedCoeff);
    }

    public override async Task Deactivate() {
        Tank.Effects.TryRemove(this);

        await Tank.UpdateModuleCooldownSpeed(1);
        Deactivated?.Invoke();
    }
}
