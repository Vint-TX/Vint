using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl.Mine;

[ProtocolId(1431927384785)]
public class MineConfigComponent(
    float beginHideDistance,
    float hideRange
) : IComponent {
    public float BeginHideDistance { get; } = beginHideDistance;
    public float HideRange { get; } = hideRange;
}
