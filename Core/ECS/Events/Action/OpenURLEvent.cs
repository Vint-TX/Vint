using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Action;

[ProtocolId(31218)]
public class OpenURLEvent(
    string url
) : IEvent {
    public string URL { get; private set; } = url;
}
