using Vint.Core.Battle.Matchmaking;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Quests;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Lobby.Impl.Arcade;

public sealed class FullRandomLobby : ArcadeLobby {
    public FullRandomLobby(MapInfo mapInfo, BattleMode battleMode, QuestManager questManager) : base(questManager) {
        ClientBattleParams clientParams = new(battleMode, GetRandomGravity(), mapInfo.Id, GetRandomBool(), GetRandomBool(), false, GetRandomMaxPlayers(), GetRandomTimer());
        Properties = new BattleProperties(BattleType.Arcade, TimeSpan.Zero, false, clientParams);
        Entity = new MatchMakingLobbyTemplate().Create(Properties);
    }

    public override ArcadeModeType ArcadeType => ArcadeModeType.WithoutDamage;
    public override BattleProperties Properties { get; protected set; }
    public override IEntity Entity { get; }

    static GravityType GetRandomGravity() => Random.Shared.NextDouble() switch {
        < 0.25 => GravityType.Moon,
        < 0.5 => GravityType.Mars,
        < 0.75 => GravityType.Earth,
        < 1 => GravityType.SuperEarth,
        _ => throw new InvalidOperationException()
    };

    static bool GetRandomBool() => MathUtils.RollTheDice(0.5);

    static int GetRandomMaxPlayers() {
        int max = Random.Shared.Next(7, 21);

        if (!int.IsEvenInteger(max))
            max += 1;

        return max;
    }

    static int GetRandomTimer() => Random.Shared.Next(7, 21);
}
