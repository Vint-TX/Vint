using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(1486980355472)]
public class PassiveModuleUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ModuleMarketItemTemplate();
}