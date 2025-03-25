using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Details;

[ProtocolId(636457330280837037)]
public class DetailMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new DetailUserItemTemplate();
}
