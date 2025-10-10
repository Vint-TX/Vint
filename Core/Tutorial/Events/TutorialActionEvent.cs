using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Tutorial.Events;

[ProtocolId(1506070003266)]
public class TutorialActionEvent : IServerEvent { // todo
    public long TutorialId { get; private set; }
    public long StepId { get; private set; }
    public TutorialAction Action { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) => throw new NotImplementedException();
}
