using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Components;

[ProtocolId(1457515023113)]
public class ConfirmedUserEmailComponent(
    string email
) : IComponent {
    public string Email { get; set; } = email;

    [Obsolete("Not used in client anymore; will be removed in the next client update")]
    public bool Subscribed { get; set; }
}
