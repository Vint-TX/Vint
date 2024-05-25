using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Quests;

[ProtocolId(1513768369538)]
public class QuestDailyBonusTemplate : EntityTemplate {
    public IEntity Create(IEntity user, bool isTaken) => Entity(null,
        builder => builder
            .AddComponent<QuestsEnabledComponent>()
            .AddGroupComponent<UserGroupComponent>(user)
            .ThenExecuteIf(_ => isTaken, entity => entity.AddComponent<TakenBonusComponent>()));
}
