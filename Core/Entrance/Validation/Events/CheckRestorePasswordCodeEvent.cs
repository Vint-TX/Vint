using System.Security.Cryptography;
using System.Text;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1460402752765)]
public class CheckRestorePasswordCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.RestorePasswordCode == null) return;

        if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(Code), Encoding.UTF8.GetBytes(connection.RestorePasswordCode))) {
            connection.RestorePasswordCodeValid = true;
            await connection.Send(new RestorePasswordCodeValidEvent(Code));
        } else {
            connection.RestorePasswordCodeValid = false;
            await connection.Send(new RestorePasswordCodeInvalidEvent(Code));
        }
    }
}
