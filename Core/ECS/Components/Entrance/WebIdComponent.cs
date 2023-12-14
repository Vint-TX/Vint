using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1479820450460)]
public class WebIdComponent : IComponent {
    public string WebId { get; private set; } = "";
    public string Utm { get; private set; } = "";
    public string GoogleAnalyticsId { get; private set; } = "";
    public string WebIdUid { get; private set; } = "";
}