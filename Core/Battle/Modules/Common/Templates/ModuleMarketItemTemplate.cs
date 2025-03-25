using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(1484905625943)]
public class ModuleMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => null!;
}
