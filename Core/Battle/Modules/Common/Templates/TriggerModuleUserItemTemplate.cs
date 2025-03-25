using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(636304361927229412)]
public class TriggerModuleUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ModuleMarketItemTemplate();
}
