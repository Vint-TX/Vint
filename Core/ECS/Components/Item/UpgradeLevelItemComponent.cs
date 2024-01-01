using Vint.Core.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Components.Item;

[ProtocolId(1436343541876)]
public class UpgradeLevelItemComponent(
    long xp
) : IComponent {
    public int Level { get; private set; } = Leveling.GetLevel(xp);
}