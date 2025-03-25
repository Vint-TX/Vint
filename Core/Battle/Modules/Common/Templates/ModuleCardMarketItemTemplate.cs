using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(636319307214133884)]
public class ModuleCardMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ModuleCardUserItemTemplate();
}
