using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Tutorial;

[ProtocolId(1506070003266)]
public class TutorialActionEvent : IServerEvent { // todo
    public long TutorialId { get; private set; }
    public long StepId { get; private set; }
    public TutorialAction Action { get; private set; }

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) => Task.CompletedTask;
}
