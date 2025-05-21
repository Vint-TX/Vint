using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460461200896)]
public class SendAgainRestorePasswordEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) { // todo email service
        if (connection.RestorePasswordCode == null)
            return Task.CompletedTask;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordCode = code;
        connection.RestorePasswordCodeValid = false;
        return Task.CompletedTask;
    }
}
