using Vint.Core.Protocol.Attributes;

namespace Vint.Core.Battles;

public class BattleProperties(
    BattleMode battleMode,
    GravityType gravity,
    long mapId,
    bool friendlyFire,
    bool killZoneEnabled,
    bool damageEnabled,
    bool disabledModules,
    int maxPlayers,
    int timeLimit,
    int scoreLimit
) {
    public BattleMode BattleMode { get; private set; } = battleMode;
    public GravityType Gravity { get; private set; } = gravity;
    public long MapId { get; private set; } = mapId;
    public bool FriendlyFire { get; private set; } = friendlyFire;
    public bool KillZoneEnabled { get; private set; } = killZoneEnabled;
    [ProtocolIgnore] public bool DamageEnabled { get; set; } = damageEnabled;
    public bool DisabledModules { get; private set; } = disabledModules;
    public int MaxPlayers { get; private set; } = maxPlayers;
    public int TimeLimit { get; private set; } = timeLimit;
    public int ScoreLimit { get; private set; } = scoreLimit;

    [ProtocolIgnore] public static IReadOnlyDictionary<GravityType, float> GravityToForce { get; set; } = new Dictionary<GravityType, float> {
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