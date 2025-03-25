using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Details;

[ProtocolId(636457331914703122)]
public class DetailUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new DetailMarketItemTemplate();
}
