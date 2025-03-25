using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Hulls;

[ProtocolId(1438603503434)]
public class TankUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new TankMarketItemTemplate();
}
