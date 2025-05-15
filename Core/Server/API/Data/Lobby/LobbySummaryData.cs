using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;

namespace Vint.Core.Server.API.Data.Lobby;

public record LobbySummaryData(
    long Id,
    long MapId,
    int PlayersCount,
    int MaxPlayersCount,
    string MapName,
    string Mode,
    BattleType Type
) {
    public static LobbySummaryData FromLobby(LobbyBase lobby) =>
        new(lobby.Entity.Id,
            lobby.Properties.GetValue(BattleProperty.MapInfo).Id,
            lobby.Players.Count,
            lobby.Properties.GetValue(BattleProperty.MaxPlayers),
            lobby.Properties.GetValue(BattleProperty.MapInfo).Name,
            lobby.Properties.GetValue(BattleProperty.BattleMode).ToString(),
            lobby.Properties.GetValue(BattleProperty.Type));
}
