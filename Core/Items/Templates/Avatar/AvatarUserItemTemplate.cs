using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Avatar;

[ProtocolId(1544694265965)]
public class AvatarUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new AvatarMarketItemTemplate();
}
