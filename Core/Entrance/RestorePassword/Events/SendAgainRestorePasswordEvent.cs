using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.RestorePassword.Components;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460461200896)]
public class SendAgainRestorePasswordEvent(
    ApiServer apiServer
) : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        RestorePasswordData? restorePasswordData = connection.RestorePasswordData;

        if (restorePasswordData == null || !connection.ClientSession.HasComponent<RestorePasswordCodeSentComponent>())
            return;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        restorePasswordData.Code = code;
        restorePasswordData.CodeValid = false;
        await apiServer.RestorePasswordRequested(restorePasswordData.PlayerId, code);
    }
}
