using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Battle.Properties;

public class BattleProperties(
    BattleType type,
    TimeSpan warmUpDuration,
    bool damageEnabled,
    ClientBattleParams clientParams
) {
    public ClientBattleParams ClientParams { get; private set; } = clientParams;
    public IEntity MapEntity { get; private set; } = ConfigManager.GetGlobalEntities("maps").Single(entity => entity.Id == clientParams.MapId);
    public MapInfo MapInfo { get; private set; } = ConfigManager.MapInfos.Single(info => info.Id == clientParams.MapId);

    public BattleType Type { get; } = type;
    public TimeSpan WarmUpDuration { get; } = warmUpDuration;
    public bool DamageEnabled { get; } = damageEnabled;

    public BattleMode BattleMode => ClientParams.BattleMode;
    public GravityType Gravity => ClientParams.Gravity;
    public bool FriendlyFire => ClientParams.FriendlyFire;
    public bool KillZoneEnabled => ClientParams.KillZoneEnabled;
    public bool DisabledModules => ClientParams.DisabledModules;
    public int MaxPlayers => ClientParams.MaxPlayers;
    public int TimeLimit => ClientParams.TimeLimit;

    public void UpdateParams(ClientBattleParams clientParams) {
        ClientParams = clientParams;
        MapInfo = ConfigManager.MapInfos.Single(info => info.Id == clientParams.MapId);
        MapEntity = ConfigManager.GetGlobalEntities("maps").Single(entity => entity.Id == clientParams.MapId);
    }
}
