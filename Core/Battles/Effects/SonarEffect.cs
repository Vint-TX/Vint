using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public class SonarEffect(
    BattleTank tank,
    int level
) : DurationEffect(tank, level, MarketConfigPath) {
    const string MarketConfigPath = "garage/module/upgrade/properties/sonar";
    
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        
        Entities.Add(new SonarEffectTemplate().Create(Tank.BattlePlayer, Duration));
        Share(Tank.BattlePlayer);
        
        Schedule(Duration, Deactivate);
    }
    
    public override void Deactivate() {
        if (!IsActive) return;
        
        Tank.Effects.TryRemove(this);
        Unshare(Tank.BattlePlayer);
        
        Entities.Clear();
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