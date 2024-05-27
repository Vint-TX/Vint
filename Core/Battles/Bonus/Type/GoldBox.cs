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
using Vint.Core.Utils;

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

    public override async Task Take(BattleTank battleTank) {
        await base.Take(battleTank);

        if (!CanTake) return;

        foreach (IPlayerConnection connection in Battle.Players
                     .Where(battlePlayer => battlePlayer.InBattle)
                     .Select(battlePlayer => battlePlayer.PlayerConnection)) {
            await connection.Send(new GoldTakenNotificationEvent(), battleTank.BattleUser);
            await connection.Unshare(RegionEntity!);
        }

        await StateManager.SetState(new None(StateManager));

        IPlayerConnection targetConnection = battleTank.BattlePlayer.PlayerConnection;

        await targetConnection.ChangeXCrystals(XCrystalsReward);

        await using (DbConnection db = new()) {
            await db.Statistics
                .Where(stats => stats.PlayerId == targetConnection.Player.Id)
                .Set(stats => stats.GoldBoxesCaught, stats => stats.GoldBoxesCaught + 1)
                .UpdateAsync();
        }

        await battleTank.SelfDestruct();
    }

    public override async Task Spawn() {
        Entity = new GoldBonusTemplate().Create(SpawnPosition, RegionEntity!, Battle.Entity);
        await StateManager.SetState(new Spawned(StateManager));
    }

    public override async Task Drop() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle)) {
            await battlePlayer.PlayerConnection.Send(new GoldScheduleNotificationEvent(""), Battle.RoundEntity);
            await battlePlayer.PlayerConnection.Share(RegionEntity!);
        }

        await StateManager.SetState(new Cooldown(StateManager, TimeSpan.FromSeconds(20)));
    }

    public override async Task Tick() {
        await base.Tick();

        if (Battle.Timer < 120 ||
            Battle.StateManager.CurrentState is not Running ||
            StateManager.CurrentState is not None) return;

        Ticks++;
        if (Ticks % DropCheckTicksCount != 0) return;

        Ticks = 0;
        float probability = Battle.MapInfo.GoldProbability;

        if (MathUtils.RollTheDice(probability))
            await Drop();
    }
}
