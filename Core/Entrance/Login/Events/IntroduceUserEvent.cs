using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Entrance.Login.Events;

public abstract class IntroduceUserEvent : IServerEvent {
    public string? Captcha { get; protected set; }

    public abstract Task Execute(IPlayerConnection connection, IEntity[] entities);
}
