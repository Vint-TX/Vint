using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1439808320725), ClientChangeable]
public class InviteComponent(
    string? inviteCode,
    bool showScreenOnEntrance
) : IComponent {
    public string? InviteCode { get; private set; } = inviteCode;
    public bool ShowScreenOnEntrance { get; private set; } = showScreenOnEntrance;
}