namespace Vint.Core.Battles;

public record struct BattleProperties(
    BattleMode BattleMode,
    GravityType GravityType,
    long MapId,
    bool FriendlyFire,
    bool KillZoneEnabled,
    bool ModulesEnabled,
    int MaxPlayers,
    int TimeLimit,
    int ScoreLimit // ???
) {
    public static IReadOnlyDictionary<GravityType, float> GravityToForce { get; } = new Dictionary<GravityType, float> {
        { GravityType.Earth, 9.81f },
        { GravityType.Moon, 1.62f },
        { GravityType.Mars, 3.71f },
        { GravityType.SuperEarth, 30f }
    };
}

public enum BattleMode : byte {
    DM = 0,
    TDM = 1,
    CTF = 2
}

public enum GravityType : byte {
    Earth = 0,
    Moon = 1,
    Mars = 2,
    SuperEarth = 3
}