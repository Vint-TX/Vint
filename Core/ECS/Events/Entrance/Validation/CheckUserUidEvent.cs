using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1437990639822)]
public class CheckUserUidEvent : IServerEvent {
    [ProtocolName("uid")] public string Username { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DatabaseContext database = new();

        if (database.Players.Any(player => player.Username == Username))
            connection.Send(new UserUidOccupiedEvent(Username));
        else connection.Send(new UserUidVacantEvent(Username));
    }
}
