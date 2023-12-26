using Vint.Core.ECS.Templates.Preset;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Premium;

[ProtocolId(1513580884352)]
public class PremiumBoostUserItemTemplate : UserEntityTemplate {
    public override MarketEntityTemplate MarketTemplate => new PresetMarketItemTemplate();
}