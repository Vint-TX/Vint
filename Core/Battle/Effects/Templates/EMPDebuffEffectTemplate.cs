using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(636250001674528715)]
public class EMPDebuffEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration) =>
        Create("battle/effect/emp", tanker, duration, false, false);
}
