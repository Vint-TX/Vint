using System.Diagnostics.CodeAnalysis;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Rewards;

[ProtocolId(636390744977660302)]
public class ChestBattleRewardComponent(
    string chestName
) : IComponent {
    [field: AllowNull, MaybeNull] public IEntity Chest {
        get {
            field ??= GlobalEntities.GetEntity("containers", chestName);
            return field;
        }
        private set;
    }
}
