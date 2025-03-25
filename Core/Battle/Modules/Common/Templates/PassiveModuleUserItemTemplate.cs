using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(1486980355472)]
public class PassiveModuleUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ModuleMarketItemTemplate();
}
