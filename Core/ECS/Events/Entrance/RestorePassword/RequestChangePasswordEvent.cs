using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460403525230)]
public class RequestChangePasswordEvent : IServerEvent {
    public string PasswordDigest { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.RestorePasswordCode == null ||
            !connection.RestorePasswordCodeValid ||
            connection.Server.DiscordBot == null) {
            connection.Player = null!;
            return;
        }

        connection.RestorePasswordCode = null;
        connection.RestorePasswordCodeValid = false;
        connection.ClientSession.RemoveComponent<RestorePasswordCodeSentComponent>();

        await connection.ChangePassword(PasswordDigest);

        connection.Player = null!;
        connection.Send(new LoginFailedEvent());
        connection.Send(new AutoLoginFailedEvent());
    }
}
