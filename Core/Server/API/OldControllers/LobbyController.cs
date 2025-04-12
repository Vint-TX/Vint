using EmbedIO;
using EmbedIO.WebApi;
using Vint.Core.Battle.Lobby;
using Vint.Core.Server.API.OldDTO.Lobby;
using Vint.Core.Server.Common.Attributes.Methods;

namespace Vint.Core.Server.API.OldControllers;

public class LobbyController(
    LobbyProcessor lobbyProcessor
) : WebApiController {
    [Get("/")]
    public IEnumerable<LobbySummaryDTO> GetLobbies() =>
        lobbyProcessor.Lobbies.Select(LobbySummaryDTO.FromLobby);

    [Get("/{id}")]
    public LobbyDetailDTO GetLobby(long id) {
        LobbyBase? lobby = lobbyProcessor.FindByLobbyId(id);

        if (lobby == null)
            throw HttpException.NotFound($"Lobby with id {id} does not exist");

        return LobbyDetailDTO.FromLobby(lobby);
    }
}
