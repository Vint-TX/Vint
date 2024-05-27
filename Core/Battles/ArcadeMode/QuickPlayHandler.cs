using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Utils;

namespace Vint.Core.Battles.ArcadeMode;

public class QuickPlayHandler(
    Battle battle
) : ArcadeModeHandler(battle) {
    public override Task Setup() {
        MapInfo mapInfo = TypeHandler.Maps.ToList().Shuffle().First();

        Battle.Properties = new BattleProperties(
            TypeHandler.BattleMode,
            GravityType.Earth,
            mapInfo.Id,
            false,
            true,
            true,
            false,
            mapInfo.MaxPlayers,
            5,
            50);

        Battle.MapInfo = mapInfo;
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == mapInfo.Id);
        Battle.LobbyEntity = new MatchMakingLobbyTemplate().Create(Battle.Properties, Battle.MapEntity);
        return Task.CompletedTask;
    }
}
