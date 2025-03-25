using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Actions;

[ProtocolId(31218)]
public class OpenURLEvent(
    string url
) : IEvent {
    public string URL { get; private set; } = url;
}
