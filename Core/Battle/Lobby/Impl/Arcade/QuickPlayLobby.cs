using Vint.Core.Battle.Lobby.Templates;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.Matchmaking;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl.Arcade;

public sealed class QuickPlayLobby : ArcadeLobby {
    public QuickPlayLobby(MapInfo mapInfo, BattleMode battleMode, QuestManager questManager) : base(questManager) {
        ClientBattleParams clientParams = new(battleMode, GravityType.Earth, mapInfo, false, true, false, 5);
        Properties = new BattleProperties(BattleType.Arcade, TimeSpan.Zero, true, clientParams);
        Entity = new MatchMakingLobbyTemplate().Create(Properties);
    }

    public override ArcadeModeType ArcadeType => ArcadeModeType.QuickPlay;
    public override BattleProperties Properties { get; protected set; }
    public override IEntity Entity { get; }
}
