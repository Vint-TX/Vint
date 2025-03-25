using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(-777019732837383198)]
public class UserExperienceComponent(
    long experience
) : IComponent {
    public long Experience { get; set; } = experience;
}
