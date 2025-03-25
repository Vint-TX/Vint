using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Items.Components;

[ProtocolId(1436343541876)]
public class UpgradeLevelItemComponent(
    long xp
) : IComponent {
    public int Level { get; private set; } = Leveling.GetLevel(xp);
}
