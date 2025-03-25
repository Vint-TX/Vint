using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.ClientSession.Components;

[ProtocolId(1439808320725), ClientChangeable]
public class InviteComponent : IComponent {
    public required string? InviteCode { get; init; }
    public required bool ShowScreenOnEntrance { get; init; }
}
