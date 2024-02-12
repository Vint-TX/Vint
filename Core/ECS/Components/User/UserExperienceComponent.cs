using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(-777019732837383198)]
public class UserExperienceComponent(
    long experience
) : IComponent {
    public long Experience { get; set; } = experience;
}