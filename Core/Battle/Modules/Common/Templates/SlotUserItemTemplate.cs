using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(1485846188251)]
public class SlotUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new SlotMarketItemTemplate();
}
