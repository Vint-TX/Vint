using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1486018775542)]
public class ArmorEffectTemplate : EffectBaseTemplate {
    public IEntity Create(string effectConfigPath, BattlePlayer battlePlayer, TimeSpan duration) =>
        Create(effectConfigPath, battlePlayer, duration, false);
}