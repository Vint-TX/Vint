using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Email.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Entrance.Login.Events;

[ProtocolId(1458846544326)]
public class IntroduceUserByEmailEvent : IntroduceUserEvent {
    public string Email { get; private set; } = null!;

    public override async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.IsLoggedIn) return;

        Email = Email.Trim();

        ILogger logger = connection.Logger.ForType<IntroduceUserByEmailEvent>();
        logger.Information("Login by email '{Email}'", Email);

        DbConnection db = new();
        Player? player = await db.GetSelfPlayerByEmail(Email);
        await db.DisposeAsync();

        if (player == null) {
            connection.Player = null!;
            await connection.Send(new EmailInvalidEvent(Email));
            await connection.Send<LoginFailedEvent>();
            return;
        }

        if (!player.EmailConfirmed) {
            connection.Player = null!;
            await connection.Send<EmailNotConfirmedEvent>();
            await connection.Send<LoginFailedEvent>();
            return;
        }

        connection.Player = player;
        await connection.Send<PersonalPasscodeEvent>();
    }
}
