using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Preset;

[ProtocolId(1493972656490)]
public class PresetMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new PresetUserItemTemplate();
}