using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(31223)]
public class DiscordOccupiedEvent(
    string discordId
) : IEvent {
    public string DiscordId => discordId;
}
