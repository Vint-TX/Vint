using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(636319307214133884)]
public class ModuleCardMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ModuleCardUserItemTemplate();
}