using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public class KamikadzeEffect(
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
        
        CanBeDeactivated = false;
        Tank.Effects.Add(this);
        
        IEntity entity = new KamikadzeEffectTemplate().Create(Tank.BattlePlayer,
            Duration,
            Battle.Properties.FriendlyFire,
            impact,
            minPercent,
            maxDamage,
            minDamage);
        
        WeaponHandler = new KamikadzeWeaponHandler(Tank,
            cooldown,
            marketEntity,
            entity,
            true,
            0,
            radius,
            minPercent,
            maxDamage,
            minDamage,
            int.MaxValue);
        
        Entities.Add(entity);
        
        Share(Tank.BattlePlayer);
        Schedule(Duration, DeactivateInternal);
    }
    
    public override void Deactivate() {
        if (!IsActive || !CanBeDeactivated) return;
        
        Tank.Effects.TryRemove(this);
        Unshare(Tank.BattlePlayer);
        
        Entities.Clear();
    }
    
    void DeactivateInternal() {
        CanBeDeactivated = true;
        Deactivate();
    }
    
    public override void Share(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;
        
        battlePlayer.PlayerConnection.Share(Entities);
    }
    
    public override void Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank != Tank) return;
        
        battlePlayer.PlayerConnection.Unshare(Entities);
    }
}