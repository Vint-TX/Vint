using Vint.Core.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.League;

[ProtocolId(1522324991586)]
public class UpdateTopLeagueInfoEvent(
    long userId
) : IEvent {
    public long UserId { get; set; } = userId;
    public double LastPlaceReputation { get; set; } = 1; // todo
    public int Place { get; set; } = Leveling.GetSeasonPlace(userId);
}