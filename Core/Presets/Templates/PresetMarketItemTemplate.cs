using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Presets.Templates;

[ProtocolId(1493972656490)]
public class PresetMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PresetUserItemTemplate();
}
