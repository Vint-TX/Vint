using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Validation;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1439375251389)]
public class IntroduceUserByUidEvent : IntroduceUserEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Login by username '{Username}'", Username);

        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Username == Username);

        if (player == null) {
            connection.Player = null!;
            connection.Send(new UidInvalidEvent());
            connection.Send(new LoginFailedEvent());
            return;
        }

        connection.Player = player;
        connection.Send(new PersonalPasscodeEvent());
    }
}