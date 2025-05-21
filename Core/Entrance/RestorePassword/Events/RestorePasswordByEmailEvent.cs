using LinqToDB;
using Serilog;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.ClientSession.Components;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460106433434)]
public class RestorePasswordByEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) { // todo email service
        ILogger logger = connection.Logger.ForType<RestorePasswordByEmailEvent>();
        logger.Warning("Restoring password '{Email}'", Email);

        DbConnection db = new();

        string? playerEmail = await db.Players
            .Where(player => player.Email == Email) // todo email confirmed check
            .Select(player => player.Email)
            .FirstOrDefaultAsync();

        await db.DisposeAsync();

        if (playerEmail == null) return;

        byte[] codeBytes = new byte[4];
        Random.Shared.NextBytes(codeBytes);
        string code = Convert.ToHexString(codeBytes);

        connection.RestorePasswordCode = code;
        await connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(playerEmail));
    }
}
