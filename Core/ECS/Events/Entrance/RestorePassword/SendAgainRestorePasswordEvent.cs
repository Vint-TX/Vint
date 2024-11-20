using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Discord;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460461200896)]
public class SendAgainRestorePasswordEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        DiscordBot? discordBot = serviceProvider.GetService<DiscordBot>();

        if (connection.RestorePasswordCode == null ||
            discordBot == null) return Task.CompletedTask;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordCode = code;
        return Task.CompletedTask;
    }
}
