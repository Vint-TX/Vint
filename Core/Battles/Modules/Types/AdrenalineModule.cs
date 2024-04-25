using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class AdrenalineModule : PassiveBattleModule, IHealthModule {
    public override string ConfigPath => "garage/module/upgrade/properties/adrenaline";
    
    public override bool ActivationCondition => Effect == null;
    
    float HpToTrigger { get; set; }
    float CooldownSpeedCoeff { get; set; }
    float DamageMultiplier { get; set; }
    
    AdrenalineEffect? Effect { get; set; }
    
    public override AdrenalineEffect GetEffect() => new(Tank, Level, CooldownSpeedCoeff, DamageMultiplier);
    
    public override void Activate() {
        if (!CanBeActivated) return;
        
        Effect = GetEffect();
        Effect.Deactivated += Deactivated;
        Effect.Activate();
        base.Activate();
    }
    
    public void OnHealthChanged(float before, float current, float max) {
        if (current > HpToTrigger || current == 0) TryDeactivate();
        else Activate();
    }
    
    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        base.Init(tank, userSlot, marketModule);
        
        HpToTrigger = Leveling.GetStat<ModuleAdrenalineEffectMaxHPPercentWorkingPropertyComponent>(ConfigPath, Level) * Tank.MaxHealth;
        CooldownSpeedCoeff = Leveling.GetStat<ModuleAdrenalineEffectCooldownSpeedCoeffPropertyComponent>(ConfigPath, Level);
        DamageMultiplier = Leveling.GetStat<ModuleDamageEffectMaxFactorPropertyComponent>(ConfigPath, Level);
    }
    
    void TryDeactivate() => Effect?.Deactivate();
    
    void Deactivated() => Effect = null;
}