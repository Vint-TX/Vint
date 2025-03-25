using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1486970297039)]
public class TurboSpeedEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration) =>
        Create("battle/effect/turbospeed", tanker, duration, false, false);
}
