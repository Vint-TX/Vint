using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Utils;

public static class ApiPlayersUtils {
    public static async Task PlayersCountChanged(this ApiServer apiServer, int playersCount) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new CountData(playersCount));

    [MessageId(40)]
    [Subscriptions(Subscriptions.Players)]
    record CountData(
        int PlayersCount
    ) : IClientDTO;
}
