using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636222384398205627)]
public class InvisibilityEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration) =>
        Create("battle/effect/invisibility", battlePlayer, duration, false);
}
