using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Lobby;

[ProtocolId(636446543585160318)]
public class CheckGiftsEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        // TODO
        Task.CompletedTask;
}
