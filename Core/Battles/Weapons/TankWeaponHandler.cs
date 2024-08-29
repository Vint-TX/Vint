using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Weapons;

public abstract class TankWeaponHandler : IWeaponHandler {
    protected TankWeaponHandler(BattleTank battleTank) {
        BattleTank = battleTank;
        BattleEntity = BattleTank.Weapon;
        MarketEntity = BattleTank.BattlePlayer.PlayerConnection.Player.CurrentPreset.Weapon;

        DamageWeakeningByDistance =
            ConfigManager.TryGetComponent(MarketConfigPath, out DamageWeakeningByDistanceComponent? damageWeakeningByDistanceComponent);

        MaxDamageDistance = damageWeakeningByDistanceComponent?.RadiusOfMaxDamage ?? 0;
        MinDamageDistance = damageWeakeningByDistanceComponent?.RadiusOfMinDamage ?? 0;
        MinDamagePercent = damageWeakeningByDistanceComponent?.MinDamagePercent ?? 0;

        OriginalWeaponRotationComponent = BattleTank.Weapon.GetComponent<WeaponRotationComponent>().Clone();
        DamageCalculator = new DamageCalculator();
    }

    public WeaponRotationComponent OriginalWeaponRotationComponent { get; }
    public IEntity BattleEntity { get; }
    public IEntity MarketEntity { get; }
    public string BattleConfigPath => BattleEntity.TemplateAccessor!.ConfigPath!;
    public string MarketConfigPath => MarketEntity.TemplateAccessor!.ConfigPath!;

    public IDamageCalculator DamageCalculator { get; }
    public BattleTank BattleTank { get; }

    public TimeSpan Cooldown { get; protected init; }

    public bool DamageWeakeningByDistance { get; }
    public float MaxDamageDistance { get; }
    public float MinDamageDistance { get; }
    public float MinDamagePercent { get; }

    public abstract int MaxHitTargets { get; }

    public abstract Task Fire(HitTarget target, int targetIndex);

    public virtual async Task OnTankEnable() =>
        await BattleEntity.AddComponent<ShootableComponent>();

    public virtual async Task OnTankDisable() =>
        await BattleEntity.RemoveComponentIfPresent<ShootableComponent>();

    public virtual Task Tick() => Task.CompletedTask;
}
