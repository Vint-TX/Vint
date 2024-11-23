using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636250001674528715)]
public class EMPDebuffEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration) =>
        Create("battle/effect/emp", battlePlayer, duration, false, false);
}
