using LinqToDB;
using Serilog;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.RestorePassword.Components;
using Vint.Core.Logging;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460106433434)]
public class RestorePasswordByEmailEvent(
    ApiServer apiServer
) : IServerEvent {
    public string Email { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Email = Email.Trim();

        ILogger logger = connection.Logger.ForType<RestorePasswordByEmailEvent>();
        logger.Warning("Restoring password '{Email}'", Email);

        DbConnection db = new();

        var player = await db.Players
            .Where(player => player.Email == Email && player.EmailConfirmed)
            .Select(player => new {
                player.Id,
                player.Email
            })
            .FirstOrDefaultAsync();

        await db.DisposeAsync();

        if (player == null) return;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordData = new RestorePasswordData(player.Id, code);

        await apiServer.RestorePasswordRequested(player.Id, code);
        await connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(player.Email!));
    }
}
