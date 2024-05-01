using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Login;

public abstract class IntroduceUserEvent : IServerEvent {
    public string? Captcha { get; protected set; }

    public abstract Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities);
}
