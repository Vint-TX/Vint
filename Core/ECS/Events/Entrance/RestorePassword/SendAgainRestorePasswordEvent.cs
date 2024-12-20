using Vint.Core.Discord;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460461200896)]
public class SendAgainRestorePasswordEvent(
    DiscordBot? discordBot
) : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.RestorePasswordCode == null || discordBot == null)
            return Task.CompletedTask;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordCode = code;
        return Task.CompletedTask;
    }
}
