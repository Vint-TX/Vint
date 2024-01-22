using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Weapons;

public abstract class WeaponHandler {
    protected WeaponHandler(BattleTank battleTank) {
        BattleTank = battleTank;
        BattleEntity = BattleTank.Weapon;
        MarketEntity = BattleTank.BattlePlayer.PlayerConnection.Player.CurrentPreset.Weapon;

        DamageWeakeningByDistance =
            ConfigManager.TryGetComponent(MarketConfigPath, out DamageWeakeningByDistanceComponent? damageWeakeningByDistanceComponent);

        MaxDamageDistance = damageWeakeningByDistanceComponent?.RadiusOfMaxDamage ?? 0;
        MinDamageDistance = damageWeakeningByDistanceComponent?.RadiusOfMinDamage ?? 0;
        MinDamagePercent = damageWeakeningByDistanceComponent?.MinDamagePercent ?? 0;

        OriginalWeaponRotationComponent = (WeaponRotationComponent)((IComponent)BattleTank.Weapon.GetComponent<WeaponRotationComponent>()).Clone();
    }

    protected WeaponRotationComponent OriginalWeaponRotationComponent { get; }
    public BattleTank BattleTank { get; }
    public IEntity BattleEntity { get; }
    public IEntity MarketEntity { get; }
    public string BattleConfigPath => BattleEntity.TemplateAccessor!.ConfigPath!;
    public string MarketConfigPath => MarketEntity.TemplateAccessor!.ConfigPath!;

    public bool CanSelfDamage => this is ThunderWeaponHandler or RicochetWeaponHandler;
    public float Cooldown { get; init; }

    public bool DamageWeakeningByDistance { get; }
    public float MaxDamageDistance { get; }
    public float MinDamageDistance { get; }
    public float MinDamagePercent { get; }

    public virtual void OnTankEnable() =>
        BattleEntity.AddComponent(new ShootableComponent());

    public virtual void OnTankDisable() =>
        BattleEntity.RemoveComponentIfPresent<ShootableComponent>();

    public virtual void Tick() { }
}