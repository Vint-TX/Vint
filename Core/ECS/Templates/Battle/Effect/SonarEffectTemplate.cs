using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636222333880646188)]
public class SonarEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration) {
        IEntity entity = Create("battle/effect/sonar", battlePlayer, duration, true);
        
        entity.AddGroupComponent<UserGroupComponent>(battlePlayer.PlayerConnection.User);
        return entity;
    }
}