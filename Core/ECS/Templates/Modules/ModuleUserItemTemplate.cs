using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(1484901449548)]
public class ModuleUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ModuleMarketItemTemplate();
}
