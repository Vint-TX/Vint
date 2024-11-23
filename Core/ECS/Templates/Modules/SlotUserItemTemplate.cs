using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(1485846188251)]
public class SlotUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new SlotMarketItemTemplate();
}
