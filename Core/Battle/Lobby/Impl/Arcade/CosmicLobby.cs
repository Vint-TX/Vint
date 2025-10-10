using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Lobby.Templates;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.Matchmaking;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl.Arcade;

public sealed class CosmicLobby : ArcadeLobby {
    public CosmicLobby(MapInfo mapInfo, BattleMode battleMode, QuestManager questManager, BotBuilder botBuilder) : base(questManager, botBuilder) {
        ClientBattleParams clientParams = new(battleMode, GravityType.Moon, mapInfo, false, true, false, 15);
        Properties = new BattleProperties(BattleType.Arcade, clientParams);
        Entity = new MatchMakingLobbyTemplate().Create(Properties);
    }

    public override ArcadeModeType ArcadeType => ArcadeModeType.CosmicBattle;
    public override BattleProperties Properties { get; protected set; }
    public override IEntity Entity { get; }
}
