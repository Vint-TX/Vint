using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Rewards;

[ProtocolId(636390744977660302)]
public class ChestBattleRewardComponent(
    string chestName
) : IComponent {
    IEntity? _chest;

    public IEntity Chest {
        get {
            _chest ??= GlobalEntities.GetEntity("containers", chestName);
            return _chest;
        }
        private set => _chest = value;
    }
}