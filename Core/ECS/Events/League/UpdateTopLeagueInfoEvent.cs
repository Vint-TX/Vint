using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.League;

[ProtocolId(1522324991586)]
public class UpdateTopLeagueInfoEvent(
    long userId,
    int place
) : IEvent {
    public long UserId { get; set; } = userId;
    public double LastPlaceReputation { get; set; } = 42; // todo
    public int Place { get; set; } = place;
}
