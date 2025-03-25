using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Presets.Templates;

[ProtocolId(1493972686116)]
public class PresetUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new PresetMarketItemTemplate();
}
