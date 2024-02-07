using System.Diagnostics;
using LinqToDB;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Type;

public class MatchmakingHandler : TypeHandler {
    public MatchmakingHandler(Battle battle) : base(battle) =>
        Maps = ConfigManager.MapInfos.Values.Where(map => map.MatchMaking && map.HasSpawnPoints(BattleMode)).ToList();

    public BattleMode BattleMode { get; } = GetRandomMode();

    List<BattlePlayer> WaitingPlayers { get; } = [];
    List<MapInfo> Maps { get; }

    public override void Setup() {
        MapInfo mapInfo = Maps.Shuffle().First();

        Battle.Properties = new BattleProperties(
            BattleMode,
            GravityType.Earth,
            mapInfo.Id,
            false,
            true,
            true,
            mapInfo.MaxPlayers,
            10,
            100);

        Battle.MapInfo = mapInfo;
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == mapInfo.Id);
        Battle.LobbyEntity = new MatchMakingLobbyTemplate().Create(
            Battle.Properties,
            Battle.MapEntity);
    }

    public override void Tick() {
        foreach (BattlePlayer battlePlayer in WaitingPlayers.ToList().Where(player => DateTimeOffset.UtcNow >= player.BattleJoinTime)) {
            battlePlayer.Init();
            WaitingPlayers.Remove(battlePlayer);
        }
    }

    public override void PlayerEntered(BattlePlayer battlePlayer) {
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        IEntity user = connection.User;

        user.AddComponent(new MatchMakingUserComponent());

        if (Battle.StateManager.CurrentState is not (WarmUp or Running)) return;

        connection.Send(new MatchMakingLobbyStartTimeEvent(battlePlayer.BattleJoinTime), user);
        WaitingPlayers.Add(battlePlayer);
    }

    public override void PlayerExited(BattlePlayer battlePlayer) {
        WaitingPlayers.Remove(battlePlayer);
        battlePlayer.PlayerConnection.User.RemoveComponentIfPresent<MatchMakingUserComponent>();

        bool battleEnded = Battle.StateManager.CurrentState is Ended;
        bool hasEnemies = Battle.Players.ToList().Any(other => other.InBattleAsTank && other.Tank!.IsEnemy(battlePlayer.Tank!));
        bool bigBattleSeries = battlePlayer.PlayerConnection.BattleSeries >= 3;

        battlePlayer.PlayerConnection.User.ChangeComponent<BattleLeaveCounterComponent>(component => {
            long lefts = component.Value;
            int needGoodBattles = component.NeedGoodBattles;

            if (battleEnded) {
                if (needGoodBattles > 0)
                    lefts = --needGoodBattles == 0 ? 0 : lefts;
                else if (lefts > 0 && bigBattleSeries)
                    lefts--;
            } else if (hasEnemies) {
                battlePlayer.PlayerConnection.BattleSeries = 0;
                lefts++;

                if (lefts >= 3) {
                    if (needGoodBattles > 0) needGoodBattles += (int)lefts / 2;
                    else needGoodBattles = 3;
                }
            }

            component.Value = lefts;
            component.NeedGoodBattles = needGoodBattles;

            using DbConnection db = new();
            
            Database.Models.Player player = battlePlayer.PlayerConnection.Player;
            player.DesertedBattlesCount = lefts;
            player.NeedGoodBattlesCount = needGoodBattles;
            
            db.Update(player);
        });
    }

    static BattleMode GetRandomMode() => Random.Shared.NextDouble() switch {
        < 0.34 => BattleMode.CTF,
        < 0.67 => BattleMode.TDM,
        < 1 => BattleMode.DM,
        _ => throw new UnreachableException()
    };
}