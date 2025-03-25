using System.Diagnostics.CodeAnalysis;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rewards.Components;

[ProtocolId(636390744977660302)]
public class ChestBattleRewardComponent(
    string chestName
) : IComponent {
    [field: AllowNull, MaybeNull] public IEntity Chest {
        get {
            field ??= GlobalEntities.GetEntity("containers", chestName);
            return field;
        }
    }
}
