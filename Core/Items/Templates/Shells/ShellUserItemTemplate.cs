using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Shells;

[ProtocolId(-1597888122960034653)]
public class ShellUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ShellMarketItemTemplate();
}
