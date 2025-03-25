using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Hulls;

[ProtocolId(1433406732656)]
public class TankMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new TankUserItemTemplate();
}
