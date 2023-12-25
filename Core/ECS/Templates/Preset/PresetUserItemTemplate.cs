using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Preset;

[ProtocolId(1493972686116)]
public class PresetUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new PresetMarketItemTemplate();
}