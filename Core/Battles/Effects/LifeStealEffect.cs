using Vint.Core.Battles.Player;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class LifeStealEffect(
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        
        Entities.Add(new LifestealEffectTemplate().Create(Tank.BattlePlayer, Duration));
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