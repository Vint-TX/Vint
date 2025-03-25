using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Containers.Templates;

[ProtocolId(1543968970810)]
public class SimpleChestUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new DonutChestMarketItemTemplate();
}
