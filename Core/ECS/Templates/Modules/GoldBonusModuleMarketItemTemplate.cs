using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(1531929900000)]
public class GoldBonusModuleMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GoldBonusModuleUserItemTemplate();
}
