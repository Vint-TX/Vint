using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(1484905625943)]
public class ModuleMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => null!;
}