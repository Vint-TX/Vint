using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Gold;

[ProtocolId(1530005871302)]
public class GoldBonusMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GoldBonusUserItemTemplate();
}
