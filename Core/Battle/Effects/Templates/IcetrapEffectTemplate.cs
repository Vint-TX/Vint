using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(636384697009346423)]
public class IceTrapEffectTemplate : MineEffectTemplate {
    protected override string ConfigPath => "battle/effect/icetrap";
}
