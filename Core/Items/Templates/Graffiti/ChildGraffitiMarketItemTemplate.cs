using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Graffiti;

[ProtocolId(636100801497439942)]
public class ChildGraffitiMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GraffitiUserItemTemplate(true);
}
