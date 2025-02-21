using Vint.Core.Battle.Matchmaking;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl.Arcade;

public sealed class CosmicLobby : ArcadeLobby {
    public CosmicLobby(MapInfo mapInfo, BattleMode battleMode, QuestManager questManager) : base(questManager) {
        ClientBattleParams clientParams = new(battleMode, GravityType.Moon, mapInfo, false, true, false, 15);
        Properties = new BattleProperties(BattleType.Arcade, TimeSpan.Zero, false, clientParams);
        Entity = new MatchMakingLobbyTemplate().Create(Properties);
    }

    public override ArcadeModeType ArcadeType => ArcadeModeType.WithoutDamage;
    public override BattleProperties Properties { get; protected set; }
    public override IEntity Entity { get; }
}
