using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(636319308428353334)]
public class ModuleCardUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ModuleCardMarketItemTemplate();
}
