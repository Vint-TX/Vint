using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Components;

[ProtocolId(1457515023113)]
public class ConfirmedUserEmailComponent(
    string email
) : IComponent {
    public string Email { get; set; } = email;
}
