using System.Diagnostics;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Utils;

namespace Vint.Core.Battles.ArcadeMode;

public class FullRandomHandler(
    Battle battle
) : ArcadeModeHandler(battle) {
    public override Task Setup() {
        MapInfo mapInfo = TypeHandler.Maps.ToList().Shuffle().First();

        Battle.Properties = new BattleProperties(
            TypeHandler.BattleMode,
            GetRandomGravity(),
            mapInfo.Id,
            GetRandomBool(),
            GetRandomBool(),
            true,
            false,
            GetRandomMaxPlayers(),
            GetRandomTimer(),
            GetRandomTimer() * 10);

        Battle.MapInfo = mapInfo;
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == mapInfo.Id);
        Battle.LobbyEntity = new MatchMakingLobbyTemplate().Create(Battle.Properties, Battle.MapEntity);
        return Task.CompletedTask;
    }

    static GravityType GetRandomGravity() => Random.Shared.NextDouble() switch {
        < 0.25 => GravityType.Moon,
        < 0.5 => GravityType.Mars,
        < 0.75 => GravityType.Earth,
        < 1 => GravityType.SuperEarth,
        _ => throw new UnreachableException()
    };

    static bool GetRandomBool() => MathUtils.RollTheDice(0.5);

    static int GetRandomMaxPlayers() {
        int max = Random.Shared.Next(8, 21);

        if (!int.IsEvenInteger(max))
            max += 1;

        return max;
    }

    static int GetRandomTimer() => Random.Shared.Next(7, 21);
}
