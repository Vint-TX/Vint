using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class ExternalImpactEffect(
    TimeSpan cooldown,
    IEntity marketEntity,
    float radius,
    float minPercent,
    float maxDamage,
    float minDamage,
    float impact,
    BattleTank tank,
    int level
) : Effect(tank, level), IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; private set; } = null!;
    
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        
        IEntity entity = new ExternalImpactEffectTemplate().Create(Tank.BattlePlayer,
            Duration,
            Battle.Properties.FriendlyFire,
            impact,
            minPercent,
            maxDamage,
            minDamage);
        
        WeaponHandler = new ExternalImpactWeaponHandler(Tank,
            cooldown,
            marketEntity,
            Entity!,
            true,
            0,
            radius,
            minPercent,
            maxDamage,
            minDamage,
            int.MaxValue);
        
        Entities.Add(entity);
        
        ShareAll();
        Schedule(Duration, Deactivate);
    }
    
    public override void Deactivate() {
        if (!IsActive) return;
        
        Tank.Effects.TryRemove(this);
        
        UnshareAll();
        Entities.Clear();
    }
}