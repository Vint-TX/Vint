using LinqToDB;
using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Validation;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1458846544326)]
public class IntroduceUserByEmailEvent : IntroduceUserEvent {
    public string Email { get; private set; } = null!;

    public override async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Login by email '{Email}'", Email);

        await using DbConnection db = new();
        Player? player = await db.Players
            .LoadWith(player => player.Modules)
            .LoadWith(player => player.DiscordLink)
            .SingleOrDefaultAsync(player => player.Email == Email);

        if (player == null) {
            connection.Player = null!;
            await connection.Send(new EmailInvalidEvent(Email));
            await connection.Send(new LoginFailedEvent());
            return;
        }

        connection.Player = player;
        await connection.Send(new PersonalPasscodeEvent());
    }
}
