using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Gold;

[ProtocolId(1530005856940)]
public class GoldBonusUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new GoldBonusMarketItemTemplate();
}
