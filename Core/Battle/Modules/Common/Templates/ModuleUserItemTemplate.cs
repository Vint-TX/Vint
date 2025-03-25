using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(1484901449548)]
public class ModuleUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ModuleMarketItemTemplate();
}
