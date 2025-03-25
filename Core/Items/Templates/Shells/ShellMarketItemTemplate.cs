using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Templates.Shells;

[ProtocolId(716181447780635764)]
public class ShellMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ShellUserItemTemplate();
}
