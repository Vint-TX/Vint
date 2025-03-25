using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Components;

[ProtocolId(1436338996992)]
public class ExperienceItemComponent(
    long xp
) : IComponent {
    public long Experience { get; set; } = xp;
}
