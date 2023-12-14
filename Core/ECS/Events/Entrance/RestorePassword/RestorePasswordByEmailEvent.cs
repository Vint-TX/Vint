using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460106433434)]
public class RestorePasswordByEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.Player = new Player(connection.Logger, Email[..Email.IndexOf('@')], Email);

        connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(Email));
    }
}