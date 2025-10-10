using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Reservation.Events;

[ProtocolId(1490780962293)]
public class ReleaseReservationInBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        throw new NotImplementedException();
}
