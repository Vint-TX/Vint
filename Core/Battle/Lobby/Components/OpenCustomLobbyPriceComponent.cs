using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Components;

[ProtocolId(1548677305789)]
public class OpenCustomLobbyPriceComponent(
    long price
) : IComponent {
    public long Price { get; private set; } = price;
}
