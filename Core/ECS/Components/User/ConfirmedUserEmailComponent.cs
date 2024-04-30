using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1457515023113)]
public class ConfirmedUserDiscordComponent(
    string username,
    bool subscribed
) : IComponent {
    [ProtocolName("Email")] public string Username { get; private set; } = username;
    public bool Subscribed { get; private set; } = subscribed;
}
