using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Item;

[ProtocolId(1436338996992)]
public class ExperienceItemComponent(
    long xp
) : IComponent {
    public long Experience { get; set; } = xp;
}