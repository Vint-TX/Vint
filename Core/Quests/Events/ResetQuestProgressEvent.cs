using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Quests.Components;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Events;

[ProtocolId(1476874341214)]
public class ResetQuestProgressEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity quest = entities.Single();

        QuestProgressComponent progressComponent = quest.GetComponent<QuestProgressComponent>();
        progressComponent.PrevValue = progressComponent.CurrentValue;

        if (progressComponent.CurrentComplete != progressComponent.PrevComplete) {
            progressComponent.PrevComplete = progressComponent.CurrentComplete;

            if (progressComponent.CurrentComplete) {
                await quest.AddComponent<RewardedQuestComponent>();
                await quest.AddComponent<CompleteQuestComponent>();
            }
        }

        await quest.ChangeComponent(progressComponent);
    }
}
