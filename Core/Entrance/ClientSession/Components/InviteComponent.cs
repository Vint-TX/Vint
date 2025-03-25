using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1439808320725), ClientChangeable]
public class InviteComponent : IComponent {
    public required string? InviteCode { get; init; }
    public required bool ShowScreenOnEntrance { get; init; }
}
