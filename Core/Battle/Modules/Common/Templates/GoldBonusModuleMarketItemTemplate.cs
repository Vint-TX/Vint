using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(1531929900000)]
public class GoldBonusModuleMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new GoldBonusModuleUserItemTemplate();
}
