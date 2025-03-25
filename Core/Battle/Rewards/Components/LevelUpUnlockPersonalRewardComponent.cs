using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rewards.Components;

[ProtocolId(1514202494334)]
public class LevelUpUnlockPersonalRewardComponent(
    List<IEntity> unlocked
) : IComponent {
    public List<IEntity> Unlocked { get; private set; } = unlocked;
}
