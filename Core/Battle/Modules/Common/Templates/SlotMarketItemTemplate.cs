using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Templates;

[ProtocolId(636390988457169067)]
public class SlotMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new SlotUserItemTemplate();
}
