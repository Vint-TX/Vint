using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1486978694968)]
public class AcceleratedGearsEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration) =>
        Create("battle/effect/acceleratedgears", battlePlayer, duration, false);
}