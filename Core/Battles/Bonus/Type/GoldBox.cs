using System.Numerics;
using LinqToDB;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Server.Battle;
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
    public override BonusType Type => BonusType.Gold;
    public override IEntity? Entity { get; protected set; }
    public override IEntity? RegionEntity { get; protected set; } = new BonusRegionTemplate().CreateGold(regionPosition);
    public override BonusConfigComponent ConfigComponent { get; } = ConfigManager.GetComponent<BonusConfigComponent>("battle/bonus/gold/cry");

    static GoldMapInfo Info => ConfigManager.CommonMapInfo.Gold;

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

        await targetConnection.PurchaseItem(Info.Reward.GetEntity(), Info.Reward.Amount, 0, false, false);

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == targetConnection.Player.Id)
            .Set(stats => stats.GoldBoxesCaught, stats => stats.GoldBoxesCaught + 1)
            .UpdateAsync();
    }

    public override async Task Spawn() {
        Entity = new GoldBonusTemplate().Create(SpawnPosition, RegionEntity!, Battle.Entity);
        await StateManager.SetState(new Spawned(StateManager));
    }

    public override bool CanBeDropped(bool force) => StateManager.CurrentState is None;

    public async Task Drop(BattlePlayer? player) {
        string username = player?.PlayerConnection.Player.Username ?? "";

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle)) {
            await battlePlayer.PlayerConnection.Send(new GoldScheduleNotificationEvent(username), Battle.RoundEntity);
            await battlePlayer.PlayerConnection.Share(RegionEntity!);
        }

        await StateManager.SetState(new Cooldown(StateManager, TimeSpan.FromSeconds(20)));
    }

    public override Task Drop() => Drop(null);

    public override async Task Tick() {
        await base.Tick();

        if (Battle.TypeHandler is not MatchmakingHandler ||
            Battle.Timer.TotalSeconds < 120 ||
            Battle.StateManager.CurrentState is not Running ||
            StateManager.CurrentState is not None) return;

        if (MathUtils.RollTheDice(Info.ProbabilityInTick))
            await Drop();
    }
}
