using Vint.Core.Battle.Effects.Components.Impl.EnergyInjection;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(636367475101866348)]
public class EnergyInjectionEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration, float percent) {
        IEntity entity = Create("battle/effect/energyinjection", tanker, duration, false, false);

        entity.AddComponent(new EnergyInjectionEffectComponent(percent));
        return entity;
    }
}
