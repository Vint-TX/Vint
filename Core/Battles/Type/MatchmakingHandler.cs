using System.Diagnostics;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Type;

public class MatchmakingHandler(
    Battle battle
) : TypeHandler(battle) {
    public BattleMode BattleMode { get; } = GetRandomMode();

    List<BattlePlayer> WaitingPlayers { get; } = [];

    public override void Setup() {
        MapInfo mapInfo = ConfigManager.MapInfos.Values
            .Where(map => map.MatchMaking)
            .ToList()
            .Shuffle()
            .First();

        Battle.Properties = new BattleProperties(
            BattleMode,
            GravityType.Earth,
            mapInfo.MapId,
            false,
            true,
            true,
            mapInfo.MaxPlayers,
            10,
            100);

        Battle.MapInfo = mapInfo;
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == mapInfo.MapId);
        Battle.LobbyEntity = new MatchMakingLobbyTemplate().Create(
            Battle.Properties,
            Battle.MapEntity,
            BattleProperties.GravityToForce[Battle.Properties.GravityType]);
    }

    public override void Tick() {
        foreach (BattlePlayer player in WaitingPlayers.ToArray()) {
            if (DateTime.UtcNow < player.BattleJoinCountdown) continue;

            player.Init();
            WaitingPlayers.Remove(player);
        }
    }

    public override void PlayerEntered(BattlePlayer player) { // todo
        player.PlayerConnection.User.AddComponent(new MatchMakingUserComponent());
        WaitingPlayers.Add(player);
    }

    public override void PlayerExited(BattlePlayer player) => throw new NotImplementedException();

    static BattleMode GetRandomMode() => new Random().Next(0, 100) switch { // todo
        //< 34 => BattleMode.CTF,
        //< 67 => BattleMode.TDM,
        <= 100 => BattleMode.DM,
        _ => throw new UnreachableException()
    };
}