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
            lobby.Properties.GetValue(BattleProperty.MapInfo).Id,
            lobby.Players.Count,
            lobby.Properties.GetValue(BattleProperty.MaxPlayers),
            lobby.Properties.GetValue(BattleProperty.FriendlyFire),
            lobby.Properties.GetValue(BattleProperty.KillZoneEnabled),
            lobby.Properties.GetValue(BattleProperty.DamageEnabled),
            !lobby.Properties.GetValue(BattleProperty.DisabledModules),
            lobby.Properties.GetValue(BattleProperty.MapInfo).Name,
            lobby.Properties.GetValue(BattleProperty.BattleMode).ToString(),
            lobby.Properties.GetValue(BattleProperty.Gravity).ToString(),
            lobby.StateManager.CurrentState.ToString(),
            lobby.Properties.GetValue(BattleProperty.Type),
            lobby.Players.Select(lobbyPlayer => PlayerSummaryData.FromPlayer(lobbyPlayer.Connection.Player)));
}
