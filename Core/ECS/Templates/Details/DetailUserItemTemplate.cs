using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Details;

[ProtocolId(636457331914703122)]
public class DetailUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new DetailMarketItemTemplate();
}