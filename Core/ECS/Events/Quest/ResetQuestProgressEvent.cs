using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Quest;

[ProtocolId(1476874341214)]
public class ResetQuestProgressEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity quest = entities.Single();

        QuestProgressComponent progressComponent = quest.GetComponent<QuestProgressComponent>();
        progressComponent.PrevValue = progressComponent.CurrentValue;

        if (progressComponent.CurrentComplete != progressComponent.PrevComplete) {
            progressComponent.PrevComplete = progressComponent.CurrentComplete;

            if (progressComponent.CurrentComplete) {
                quest.AddComponent<RewardedQuestComponent>();
                quest.AddComponent<CompleteQuestComponent>();
            }
        }

        quest.ChangeComponent(progressComponent);
        return Task.CompletedTask;
    }
}
