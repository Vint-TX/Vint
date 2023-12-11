using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1439375251389)]
public class IntroduceUserByUidEvent : IntroduceUserEvent {
    [ProtocolName("uid")] public string Username { get; private set; } = null!;

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Login by username '{Username}'", Username);

        connection.Player = new Player(logger, Username, $"{Username}@placeholder.com");
        connection.Send(new PersonalPasscodeEvent());
    }
}
