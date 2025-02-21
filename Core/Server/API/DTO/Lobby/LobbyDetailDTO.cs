using Vint.Core.Battle.Lobby;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.API.DTO.Player;

namespace Vint.Core.Server.API.DTO.Lobby;

public record LobbyDetailDTO(
    long Id,
    long MapId,
    int PlayersCount,
    int MaxPlayersCount,
    bool FriendlyFire,
    bool KillZoneEnabled,
    bool DamageEnabled,
    bool ModulesEnabled,
    string MapName,
    string Mode,
    string Gravity,
    string State,
    BattleType Type,
    IEnumerable<PlayerSummaryDTO> Players
) {
    public static LobbyDetailDTO FromLobby(LobbyBase lobby) =>
        new(lobby.Entity.Id,
            lobby.Properties.MapInfo.Id,
            lobby.Players.Count,
            lobby.Properties.MaxPlayers,
            lobby.Properties.FriendlyFire,
            lobby.Properties.KillZoneEnabled,
            lobby.Properties.DamageEnabled,
            !lobby.Properties.DisabledModules,
            lobby.Properties.MapInfo.Name,
            lobby.Properties.BattleMode.ToString(),
            lobby.Properties.Gravity.ToString(),
            lobby.StateManager.CurrentState.ToString(),
            lobby.Properties.Type,
            lobby.Players.Select(lobbyPlayer => PlayerSummaryDTO.FromPlayer(lobbyPlayer.Connection.Player)));
}
