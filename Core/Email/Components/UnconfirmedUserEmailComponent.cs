using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Components;

[ProtocolId(635902862624765629)]
public class UnconfirmedUserEmailComponent(
    string email
) : IComponent {
    public string Email { get; set; } = email;
}
