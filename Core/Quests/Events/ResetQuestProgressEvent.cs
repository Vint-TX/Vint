using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Quest;

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
