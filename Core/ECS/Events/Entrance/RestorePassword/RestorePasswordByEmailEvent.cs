using Serilog;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.ECS.Events.Entrance.Validation;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460106433434)]
public class RestorePasswordByEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Restoring password '{Email}'", Email);

        using DatabaseContext database = new();
        Player player = database.Players.Single(player => player.Email == Email);

        connection.Player = player;

        //todo

        connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(Email));
    }
}
