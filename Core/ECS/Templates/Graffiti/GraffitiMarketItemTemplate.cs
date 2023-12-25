using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Graffiti;

[ProtocolId(636100801770520539)]
public class GraffitiMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GraffitiUserItemTemplate(false);
}