using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Graffiti;

[ProtocolId(636100801716991373)]
public class GraffitiUserItemTemplate(
    bool isChild
) : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => isChild
        ? new ChildGraffitiMarketItemTemplate()
        : new GraffitiMarketItemTemplate();
}
