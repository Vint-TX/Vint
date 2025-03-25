using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(636408122917164205)]
public class DonutChestMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new SimpleChestUserItemTemplate();
}
