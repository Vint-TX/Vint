using System.Buffers.Text;
using System.Security.Cryptography;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1458846544326)]
public class IntroduceUserByEmailEvent : IntroduceUserEvent {
    public string Email { get; private set; } = null!;

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Login by email '{Email}'", Email);

        connection.Player = new Player(logger, Email[..Email.IndexOf('@')], Email);
        connection.Send(new PersonalPasscodeEvent());
    }
}
