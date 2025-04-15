using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;
using Vint.Core.Server.API.Data.Player;

namespace Vint.Core.Server.API.Data.Lobby;

public record LobbyDetailData(
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
    IEnumerable<PlayerSummaryData> Players
) {
    public static LobbyDetailData FromLobby(LobbyBase lobby) =>
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
            lobby.Players.Select(lobbyPlayer => PlayerSummaryData.FromPlayer(lobbyPlayer.Connection.Player)));
}
