using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460106433434)]
public class RestorePasswordByDiscordEvent : IServerEvent {
    [ProtocolName("Email")] public string Username { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Warning("Restoring password '{Username}'", Username);
/*
        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Email == Email);

        if (player == null) return;

        connection.Player = player;

        //todo

        connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(Email));*/
    }
}
