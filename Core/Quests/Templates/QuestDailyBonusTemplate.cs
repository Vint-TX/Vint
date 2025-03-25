using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Quests.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Quests.Templates;

[ProtocolId(1513768369538)]
public class QuestDailyBonusTemplate : EntityTemplate {
    public IEntity Create(IEntity user, bool isTaken) => Entity(null,
        builder => builder
            .AddComponent<QuestsEnabledComponent>()
            .AddGroupComponent<UserGroupComponent>(user)
            .ThenExecuteIf(_ => isTaken, entity => entity.AddComponent<TakenBonusComponent>()));
}
