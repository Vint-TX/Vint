using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Rewards;

[ProtocolId(1514202494334)]
public class LevelUpUnlockPersonalRewardComponent(
    List<IEntity> unlocked
) : IComponent {
    public List<IEntity> Unlocked { get; private set; } = unlocked;
}