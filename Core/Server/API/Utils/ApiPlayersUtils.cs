using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Utils;

public static class ApiPlayersUtils {
    public static async Task PlayersCountChanged(this ApiServer apiServer, int playersCount) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new CountData(playersCount));

    public static async Task RestorePasswordRequested(this ApiServer apiServer, long playerId, string code) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new RestorePasswordRequestedData(playerId, code));

    public static async Task PasswordChanged(this ApiServer apiServer, long playerId) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new PasswordChangedData(playerId));

    [MessageId(40)]
    [Subscriptions(Subscriptions.Players)]
    record CountData(
        int PlayersCount
    ) : IClientDTO;

    [MessageId(41)]
    [Subscriptions(Subscriptions.Players)]
    record RestorePasswordRequestedData(
        long PlayerId,
        string Code
    ) : IClientDTO;

    [MessageId(42)]
    [Subscriptions(Subscriptions.Players)]
    record PasswordChangedData(
        long PlayerId
    ) : IClientDTO;
}
