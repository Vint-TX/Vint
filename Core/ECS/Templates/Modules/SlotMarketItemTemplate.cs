using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(636390988457169067)]
public class SlotMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new SlotUserItemTemplate();
}
