using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(-5477085396086342998)]
public class UserUidComponent(
    string username
) : IComponent {
    [ProtocolName("Uid")] public string Username { get; set; } = username;
}
