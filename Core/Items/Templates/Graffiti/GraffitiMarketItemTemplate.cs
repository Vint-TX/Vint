using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Graffiti;

[ProtocolId(636100801770520539)]
public class GraffitiMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GraffitiUserItemTemplate(false);
}
