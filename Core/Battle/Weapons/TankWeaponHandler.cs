using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Weapons;

public abstract class TankWeaponHandler : IWeaponHandler {
    protected TankWeaponHandler(BattleTank battleTank) {
        BattleTank = battleTank;
        BattleEntity = BattleTank.Entities.Weapon;
        MarketEntity = BattleTank.Tanker.Connection.Player.CurrentPreset.Weapon;

        DamageWeakeningByDistance =
            ConfigManager.TryGetComponent(MarketConfigPath, out DamageWeakeningByDistanceComponent? damageWeakeningByDistanceComponent);

        MaxDamageDistance = damageWeakeningByDistanceComponent?.RadiusOfMaxDamage ?? 0;
        MinDamageDistance = damageWeakeningByDistanceComponent?.RadiusOfMinDamage ?? 0;
        MinDamagePercent = damageWeakeningByDistanceComponent?.MinDamagePercent ?? 0;

        WeaponRotationComponent = BattleEntity
            .GetComponent<WeaponRotationComponent>()
            .Clone();
    }

    public WeaponRotationComponent WeaponRotationComponent { get; }
    public IEntity BattleEntity { get; }
    public IEntity MarketEntity { get; }
    public string BattleConfigPath => BattleEntity.TemplateAccessor!.ConfigPath!;
    public string MarketConfigPath => MarketEntity.TemplateAccessor!.ConfigPath!;

    public IDamageCalculator DamageCalculator => BattleTank.Round.DamageCalculator;
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

    public virtual Task Tick(TimeSpan deltaTime) => Task.CompletedTask;
}
