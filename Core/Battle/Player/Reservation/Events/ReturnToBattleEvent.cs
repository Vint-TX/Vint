using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Reservation.Events;

[ProtocolId(1490604380473)]
public class ReturnToBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        throw new NotImplementedException();
}
