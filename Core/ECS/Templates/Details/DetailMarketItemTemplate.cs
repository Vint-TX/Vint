using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Details;

[ProtocolId(636457330280837037)]
public class DetailMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new DetailUserItemTemplate();
}