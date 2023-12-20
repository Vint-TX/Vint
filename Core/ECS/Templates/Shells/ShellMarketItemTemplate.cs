using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Shells;

[ProtocolId(716181447780635764)]
public class ShellMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new ShellUserItemTemplate();
}