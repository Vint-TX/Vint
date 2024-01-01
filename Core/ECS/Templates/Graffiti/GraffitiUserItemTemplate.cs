using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Graffiti;

[ProtocolId(636100801716991373)]
public class GraffitiUserItemTemplate(
    bool isChild
) : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => isChild
                                                               ? new ChildGraffitiMarketItemTemplate()
                                                               : new GraffitiMarketItemTemplate();
}