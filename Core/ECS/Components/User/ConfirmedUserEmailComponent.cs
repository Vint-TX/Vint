using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1457515023113)]
public class ConfirmedUserEmailComponent(
    string email,
    bool subscribed
) : IComponent {
    public string Email { get; private set; } = email;
    public bool Subscribed { get; private set; } = subscribed;
}