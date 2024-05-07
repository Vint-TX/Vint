using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1503314606668)]
public class ForceFieldEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration) =>
        Create("battle/effect/forcefield", battlePlayer, duration, true);
}
