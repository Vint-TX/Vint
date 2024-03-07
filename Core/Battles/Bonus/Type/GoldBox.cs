using System.Numerics;
using LinqToDB;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Bonus;
using Vint.Core.ECS.Templates.Battle.Bonus;
using Vint.Core.Server;

namespace Vint.Core.Battles.Bonus.Type;

public sealed class GoldBox(
    Battle battle,
    Vector3 regionPosition,
    bool hasParachute
) : BonusBox(battle, regionPosition, hasParachute) {
    const int XCrystalsReward = 100;
    const int DropCheckTicksCount = 1000;
    public override BonusType Type => BonusType.Gold;
    public override IEntity? Entity { get; protected set; }
    public override IEntity? RegionEntity { get; protected set; } = new BonusRegionTemplate().CreateGold(regionPosition);
    public override BonusConfigComponent ConfigComponent { get; } = ConfigManager.GetComponent<BonusConfigComponent>("battle/bonus/gold/cry");

    int Ticks { get; set; }

    public override void Take(BattleTank battleTank) {
        base.Take(battleTank);

        if (!CanTake) return;

        foreach (IPlayerConnection connection in Battle.Players
                     .Where(battlePlayer => battlePlayer.InBattle)
                     .Select(battlePlayer => battlePlayer.PlayerConnection)) {
            connection.Send(new GoldTakenNotificationEvent(), battleTank.BattleUser);
            connection.Unshare(RegionEntity!);
        }

        StateManager.SetState(new None(StateManager));

        IPlayerConnection targetConnection = battleTank.BattlePlayer.PlayerConnection;

        using (DbConnection db = new()) {
            db.Statistics
                .Where(stats => stats.PlayerId == targetConnection.Player.Id)
                .Set(stats => stats.GoldBoxesCaught, stats => stats.GoldBoxesCaught + 1)
                .Update();
        }

        targetConnection.SetXCrystals(targetConnection.Player.XCrystals + XCrystalsReward);
    }

    public override void Spawn() {
        Entity = new GoldBonusTemplate().Create(SpawnPosition, RegionEntity!, Battle.Entity);
        StateManager.SetState(new Spawned(StateManager));
    }

    void Drop() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle)) {
            battlePlayer.PlayerConnection.Send(new GoldScheduleNotificationEvent(""), Battle.RoundEntity);
            battlePlayer.PlayerConnection.Share(RegionEntity!);
        }

        StateManager.SetState(new Cooldown(StateManager, TimeSpan.FromSeconds(30)));
    }

    public override void Tick() {
        base.Tick();
        
        if (StateManager.CurrentState is not None) return;

        Ticks++;
        if (Ticks % DropCheckTicksCount != 0) return;

        Ticks = 0;
        float probability = Battle.MapInfo.GoldProbability;

        if (Random.Shared.NextDouble() <= probability)
            Drop();
    }
}