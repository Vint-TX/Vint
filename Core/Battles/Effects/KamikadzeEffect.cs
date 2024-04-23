using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public class KamikadzeEffect(
    KamikadzeWeaponHandler weaponHandler,
    BattleTank tank,
    int level
) : Effect(tank, level), IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; } = weaponHandler;
    
    public override void Activate() {
        if (IsActive) return;
        
        CanBeDeactivated = false;
        Tank.Effects.Add(this);
        
        Entities.Add(new KamikadzeEffectTemplate().Create(Tank.BattlePlayer,
            Duration,
            Battle.Properties.FriendlyFire,
            weaponHandler.Impact,
            weaponHandler.MinDamagePercent,
            weaponHandler.MaxDamageDistance,
            weaponHandler.MinDamageDistance));
        
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