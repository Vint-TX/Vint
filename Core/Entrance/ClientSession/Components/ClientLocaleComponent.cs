using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.ClientSession.Components;

[ProtocolId(1453796862447), ClientAddable]
public class ClientLocaleComponent : IComponent {
    public string LocaleCode { get; private set; } = null!;
}
