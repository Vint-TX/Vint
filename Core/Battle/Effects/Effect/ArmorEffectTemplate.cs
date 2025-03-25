using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1486018775542)]
public class ArmorEffectTemplate : EffectBaseTemplate {
    public IEntity Create(string effectConfigPath, Tanker tanker, TimeSpan duration) =>
        Create(effectConfigPath, tanker, duration, false, false);
}
