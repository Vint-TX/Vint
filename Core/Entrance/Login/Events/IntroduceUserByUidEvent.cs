using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Entrance.Validation.Events;
using Vint.Core.Logging;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Login.Events;

[ProtocolId(1439375251389)]
public class IntroduceUserByUidEvent : IntroduceUserEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;

    public override async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.IsLoggedIn) return;

        ILogger logger = connection.Logger.ForType<IntroduceUserByUidEvent>();
        logger.Information("Login by username '{Username}'", Username);

        DbConnection db = new();
        Player? player = await db.GetSelfPlayerByUsername(Username);
        await db.DisposeAsync();

        if (player == null) {
            connection.Player = null!;
            await connection.Send<UidInvalidEvent>();
            await connection.Send<LoginFailedEvent>();
            return;
        }

        connection.Player = player;
        await connection.Send<PersonalPasscodeEvent>();
    }
}
