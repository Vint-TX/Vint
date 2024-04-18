using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class KamikadzeModule : TriggerBattleModule, IDeathModule {
    public override string ConfigPath => "garage/module/upgrade/properties/kamikadze";
    
    public override KamikadzeEffect GetEffect() => new(WeaponHandler, Tank, Level);
    
    KamikadzeWeaponHandler WeaponHandler { get; set; } = null!;
    
    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        base.Init(tank, userSlot, marketModule);
        
        float impact = Leveling.GetStat<ModuleEffectImpactPropertyComponent>(ConfigPath, Level);
        float radius = Leveling.GetStat<ModuleEffectSplashRadiusPropertyComponent>(ConfigPath, Level);
        float minPercent = Leveling.GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>(ConfigPath, Level) * 100;
        float minDamage = Leveling.GetStat<ModuleEffectMinDamagePropertyComponent>(ConfigPath, Level);
        float maxDamage = Leveling.GetStat<ModuleEffectMaxDamagePropertyComponent>(ConfigPath, Level);
        
        WeaponHandler = new KamikadzeWeaponHandler(Tank,
            Cooldown,
            MarketEntity,
            true,
            0,
            radius,
            minPercent,
            maxDamage,
            minDamage,
            impact,
            int.MaxValue);
    }
    
    public override void Activate() {
        if (!CanBeActivated) return;
        
        KamikadzeEffect? effect = Tank.Effects.OfType<KamikadzeEffect>().SingleOrDefault();
        
        if (effect != null) return;
        
        base.Activate();
        GetEffect().Activate();
    }
    
    public void OnDeath() => Activate();
}