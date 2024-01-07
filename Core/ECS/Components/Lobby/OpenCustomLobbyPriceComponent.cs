using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Lobby;

[ProtocolId(1548677305789)]
public class OpenCustomLobbyPriceComponent(
    long price
) : IComponent {
    public long Price { get; private set; } = price;
}