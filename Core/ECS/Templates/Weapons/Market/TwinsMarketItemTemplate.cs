using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Market;

[ProtocolId(1435138663254)]
public class TwinsMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new TwinsUserItemTemplate();
}