using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Avatar;

[ProtocolId(1544694405895)]
public class AvatarMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new AvatarUserItemTemplate();
}