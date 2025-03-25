using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.DailyBonus;

[ProtocolId(636459062089487176)]
public class UserDailyBonusZoneComponent(
    int zoneNumber
) : IComponent {
    public int ZoneNumber { get; set; } = zoneNumber;
}
