using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Shells;

[ProtocolId(-1597888122960034653)]
public class ShellUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new ShellMarketItemTemplate();
}