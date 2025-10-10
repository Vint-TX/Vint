using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.ClientSession;
using Vint.Core.Entrance.Login.Events;
using Vint.Core.Entrance.RestorePassword.Components;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460403525230)]
public class RequestChangePasswordEvent(
    ApiServer apiServer
) : IServerEvent {
    public string PasswordDigest { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        RestorePasswordData? restorePasswordData = connection.RestorePasswordData;

        if (restorePasswordData is not { CodeValid: true } || !connection.ClientSession.HasComponent<RestorePasswordCodeSentComponent>())
            return;

        connection.RestorePasswordData = null;
        await connection.ClientSession.RemoveComponent<RestorePasswordCodeSentComponent>();

        byte[] passwordHash = new Encryption().RsaDecrypt(Convert.FromBase64String(PasswordDigest));

        await using (DbConnection db = new()) {
            await db.Players
                .Where(player => player.Id == restorePasswordData.PlayerId)
                .Set(player => player.PasswordHash, passwordHash)
                .UpdateAsync();
        }

        await apiServer.PasswordChanged(restorePasswordData.PlayerId, DateTimeOffset.UtcNow);

        await connection.Send<LoginFailedEvent>();
        await connection.Send<AutoLoginFailedEvent>();
    }
}
