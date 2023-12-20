using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Avatar;

[ProtocolId(1544694265965)]
public class AvatarUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new AvatarMarketItemTemplate();
}