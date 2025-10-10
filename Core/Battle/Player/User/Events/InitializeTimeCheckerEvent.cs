using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.User.Events;

[ProtocolId(1461735527769)]
public class InitializeTimeCheckerEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        Task.CompletedTask; // todo ??
}
