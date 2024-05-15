using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460461200896)]
public class SendAgainRestorePasswordEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.RestorePasswordCode == null ||
            connection.Server.DiscordBot == null) return Task.CompletedTask;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordCode = code;
        return Task.CompletedTask;
    }
}
