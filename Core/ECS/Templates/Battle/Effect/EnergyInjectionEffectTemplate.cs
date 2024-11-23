using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect.Type.EnergyInjection;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636367475101866348)]
public class EnergyInjectionEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration, float percent) {
        IEntity entity = Create("battle/effect/energyinjection", battlePlayer, duration, false, false);

        entity.AddComponent(new EnergyInjectionEffectComponent(percent));
        return entity;
    }
}
