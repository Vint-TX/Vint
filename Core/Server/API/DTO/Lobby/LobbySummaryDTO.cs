using Vint.Core.Battle.Lobby;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Server.API.DTO.Lobby;

public record LobbySummaryDTO(
    long Id,
    long MapId,
    int PlayersCount,
    int MaxPlayersCount,
    string MapName,
    string Mode,
    BattleType Type
) {
    public static LobbySummaryDTO FromLobby(LobbyBase lobby) =>
        new(lobby.Entity.Id,
            lobby.Properties.MapInfo.Id,
            lobby.Players.Count,
            lobby.Properties.MaxPlayers,
            lobby.Properties.MapInfo.Name,
            lobby.Properties.BattleMode.ToString(),
            lobby.Properties.Type);
}
