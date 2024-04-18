using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public class RageEffect(
    TimeSpan decreaseCooldownPerKill,
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        
        Entities.Add(new RageEffectTemplate().Create(Tank.BattlePlayer, Duration, decreaseCooldownPerKill));
        Share(Tank.BattlePlayer);
        
        LastActivationTime = DateTimeOffset.UtcNow;
        Schedule(Duration, Deactivate);
    }
    
    public override void Deactivate() {
        if (!IsActive) return;
        
        Tank.Effects.TryRemove(this);
        Unshare(Tank.BattlePlayer);
        
        Entities.Clear();
        LastActivationTime = default;
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