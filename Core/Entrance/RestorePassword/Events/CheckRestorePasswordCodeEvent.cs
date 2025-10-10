using System.Security.Cryptography;
using System.Text;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.RestorePassword.Components;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460402752765)]
public class CheckRestorePasswordCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        RestorePasswordData? restorePasswordData = connection.RestorePasswordData;

        if (restorePasswordData == null || !connection.ClientSession.HasComponent<RestorePasswordCodeSentComponent>())
            return;

        if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(Code), Encoding.UTF8.GetBytes(restorePasswordData.Code))) {
            restorePasswordData.CodeValid = true;
            await connection.Send(new RestorePasswordCodeValidEvent(Code));
        } else {
            restorePasswordData.CodeValid = false;
            await connection.Send(new RestorePasswordCodeInvalidEvent(Code));
        }
    }
}
