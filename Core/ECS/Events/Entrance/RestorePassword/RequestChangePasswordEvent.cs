using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460403525230)]
public class RequestChangePasswordEvent : IServerEvent {
    public string PasswordDigest { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        await connection.ChangePassword(PasswordDigest);
        await connection.Login(true, true, HardwareFingerprint);
    }
}
