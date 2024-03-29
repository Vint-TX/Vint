using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1453796862447), ClientAddable]
public class ClientLocaleComponent : IComponent {
    public string LocaleCode { get; private set; } = null!;
}