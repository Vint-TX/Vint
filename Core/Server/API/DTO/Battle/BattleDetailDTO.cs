using Vint.Core.Battles.Type;
using Vint.Core.Server.API.DTO.Player;

namespace Vint.Core.Server.API.DTO.Battle;

public record BattleDetailDTO(
    long Id,
    long LobbyId,
    long MapId,
    int PlayersCount,
    int MaxPlayersCount,
    bool Matchmaking,
    bool FriendlyFire,
    bool KillZoneEnabled,
    bool DamageEnabled,
    bool ModulesEnabled,
    string MapName,
    string Mode,
    string Gravity,
    string State,
    TimeSpan Timer,
    IEnumerable<PlayerSummaryDTO> Players
) {
    public static BattleDetailDTO FromBattle(Battles.Battle battle) =>
        new(battle.Id,
            battle.LobbyId,
            battle.MapInfo.Id,
            battle.Players.Count,
            battle.Properties.MaxPlayers,
            battle.TypeHandler is MatchmakingHandler,
            battle.Properties.FriendlyFire,
            battle.Properties.KillZoneEnabled,
            battle.Properties.DamageEnabled,
            !battle.Properties.DisabledModules,
            battle.MapInfo.Name,
            battle.Properties.BattleMode.ToString(),
            battle.Properties.Gravity.ToString(),
            battle.StateManager.CurrentState.ToString(),
            battle.Timer,
            battle.Players.Select(battlePlayer => PlayerSummaryDTO.FromPlayer(battlePlayer.PlayerConnection.Player)));
}
