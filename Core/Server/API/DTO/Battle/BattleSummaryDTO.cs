using Vint.Core.Battles.Type;

namespace Vint.Core.Server.API.DTO.Battle;

public record BattleSummaryDTO(
    long Id,
    long LobbyId,
    long MapId,
    int PlayersCount,
    int MaxPlayersCount,
    bool Matchmaking,
    string MapName,
    string Mode
) {
    public static BattleSummaryDTO FromBattle(Battles.Battle battle) =>
        new(battle.Id,
            battle.LobbyId,
            battle.MapInfo.Id,
            battle.Players.Count,
            battle.Properties.MaxPlayers,
            battle.TypeHandler is MatchmakingHandler,
            battle.MapInfo.Name,
            battle.Properties.BattleMode.ToString());
}
