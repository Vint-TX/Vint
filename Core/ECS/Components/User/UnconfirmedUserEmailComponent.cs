using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(635902862624765629)]
public class UnconfirmedUserDiscordComponent(
    string username
) : IComponent {
    [ProtocolName("Email")] public string Username { get; set; } = username;
}
