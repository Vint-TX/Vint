using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636384697009346423)]
public class IceTrapEffectTemplate : MineEffectTemplate {
    protected override string ConfigPath => "battle/effect/icetrap";
}
