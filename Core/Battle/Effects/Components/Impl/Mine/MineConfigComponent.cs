using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type.Mine;

[ProtocolId(1431927384785)]
public class MineConfigComponent(
    float beginHideDistance,
    float hideRange
) : IComponent {
    public float BeginHideDistance { get; } = beginHideDistance;
    public float HideRange { get; } = hideRange;
}
