using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Covers;

[ProtocolId(636287154959625373)]
public class WeaponPaintUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new WeaponPaintMarketItemTemplate();
}