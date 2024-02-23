using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(-5477085396086342998)]
public class UserUidComponent(
    string username
) : IComponent {
    [ProtocolName("Uid")] public string Username { get; set; } = username;
}