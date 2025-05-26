using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Utils;

public static class ApiPlayersUtils {
    public static async Task PlayersCountChanged(this ApiServer apiServer, int playersCount) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new CountData(playersCount));

    public static async Task RestorePasswordRequested(this ApiServer apiServer, long playerId, string code) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new RestorePasswordRequestedData(playerId, code));

    public static async Task PasswordChanged(this ApiServer apiServer, long playerId, DateTimeOffset changedAt) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new PasswordChangedData(playerId, changedAt));

    public static async Task NewPlayerRegistered(this ApiServer apiServer, long playerId, string confirmationUrl) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new NewPlayerRegisteredData(playerId, confirmationUrl));

    public static async Task EmailChangeRequested(this ApiServer apiServer, long playerId, string oldEmail, string newEmail, string receiverEmail, string confirmationUrl) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new EmailChangeRequestedData(playerId, oldEmail, newEmail, receiverEmail, confirmationUrl));

    public static async Task EmailChanged(this ApiServer apiServer, long playerId, string oldEmail, string newEmail, DateTimeOffset changedAt) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new EmailChangedData(playerId, oldEmail, newEmail, changedAt));

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
        long PlayerId,
        string ConfirmationUrl
    ) : IClientDTO;

    [MessageId(44)]
    [Subscriptions(Subscriptions.Players)]
    record EmailChangedData(
        long PlayerId,
        string OldEmail,
        string NewEmail,
        DateTimeOffset ChangedAt
    ) : IClientDTO;

    [MessageId(45)]
    [Subscriptions(Subscriptions.Players)]
    record EmailChangeRequestedData(
        long PlayerId,
        string OldEmail,
        string NewEmail,
        string ReceiverEmail,
        string ConfirmationUrl
    ) : IClientDTO;
}
