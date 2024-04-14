using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class SonarModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/sonar";
    
    public override Effect GetEffect() => new SonarEffect(Tank, Level);
    
    public override void Activate() {
        if (!CanBeActivated) return;
        
        SonarEffect? effect = Tank.Effects.OfType<SonarEffect>().SingleOrDefault();
        
        if (effect != null) return;
        
        base.Activate();
        GetEffect().Activate();
    }
}