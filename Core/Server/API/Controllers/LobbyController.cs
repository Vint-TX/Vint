using Vint.Core.Battle.Lobby;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Lobby;
using Vint.Core.Server.API.Data.Status;

namespace Vint.Core.Server.API.Controllers;

public class LobbyController(
    LobbyProcessor lobbyProcessor
) : IApiController {
    [MessageId(8)]
    public IClientDTO GetLobbies() =>
        SuccessDTO.Ok(lobbyProcessor.Lobbies.Select(LobbySummaryData.FromLobby));

    [MessageId(9)]
    public IClientDTO GetLobby(long id) {
        LobbyBase? lobby = lobbyProcessor.FindByLobbyId(id);

        if (lobby == null)
            return ErrorDTO.NotFound($"Lobby with id {id} does not exist");

        return SuccessDTO.Ok(LobbyDetailData.FromLobby(lobby));
    }
}
