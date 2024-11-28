namespace Vint.Core.Server.API.DTO.Server;

public record CountDTO(
    int Connections,
    int Players,
    int MatchmakingBattles,
    int ArcadeBattles,
    int CustomBattles
);
