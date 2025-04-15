namespace Vint.Core.Server.API.Data.Server;

public record CountData(
    int Connections,
    int Players,
    int MatchmakingBattles,
    int ArcadeBattles,
    int CustomBattles
);
