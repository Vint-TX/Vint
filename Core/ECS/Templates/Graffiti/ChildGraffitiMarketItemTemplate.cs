using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Graffiti;

[ProtocolId(636100801497439942)]
public class ChildGraffitiMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GraffitiUserItemTemplate(true);
}