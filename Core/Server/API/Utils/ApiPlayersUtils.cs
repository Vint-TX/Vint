using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Player;

namespace Vint.Core.Server.API.Utils;

public static class ApiPlayersUtils {
    public static async Task PlayersCountChanged(this ApiServer apiServer, int playersCount) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new CountData(playersCount));

    public static async Task RestorePasswordRequested(this ApiServer apiServer, long playerId, string code) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new RestorePasswordRequestedData(playerId, code));

    public static async Task PasswordChanged(this ApiServer apiServer, long playerId, DateTimeOffset changedAt) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new PasswordChangedData(playerId, changedAt));

    public static async Task NewPlayerRegistered(this ApiServer apiServer, long playerId) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new NewPlayerRegisteredData(playerId));

    public static async Task EmailChanged(this ApiServer apiServer, long playerId, string oldEmail, DateTimeOffset changedAt) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new EmailChangedData(playerId, oldEmail, changedAt));

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
        long PlayerId,
        DateTimeOffset ChangedAt
    ) : IClientDTO;

    [MessageId(43)]
    [Subscriptions(Subscriptions.Players)]
    record NewPlayerRegisteredData(
        long PlayerId
    ) : IClientDTO;

    [MessageId(44)]
    [Subscriptions(Subscriptions.Players)]
    record EmailChangedData(
        long PlayerId,
        string OldEmail,
        DateTimeOffset ChangedAt
    ) : IClientDTO;
}
