using System.Numerics;
using LinqToDB;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Server.Battle;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Bonus;
using Vint.Core.ECS.Templates.Battle.Bonus;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Bonus.Type;

public sealed class GoldBox(
    Round round,
    Vector3 regionPosition,
    bool hasParachute
) : BonusBox(round, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Gold;
    public override IEntity? Entity { get; protected set; }
    public override Lazy<IEntity> RegionEntity { get; protected set; } = new(new BonusRegionTemplate().CreateGold(regionPosition));
    public override BonusConfigComponent ConfigComponent { get; } = ConfigManager.GetComponent<BonusConfigComponent>("battle/bonus/gold/cry");

    static GoldMapInfo Info => ConfigManager.CommonMapInfo.Gold;

    public override async Task Take(BattleTank battleTank) {
        await base.Take(battleTank);

        if (!CanTake) return;

        ICollection<BattlePlayer> players = Round.Players;
        await players.Send(new GoldTakenNotificationEvent(), battleTank.Entities.BattleUser);
        await players.Unshare(RegionEntity.Value);

        await StateManager.SetState(new None(StateManager));

        IPlayerConnection targetConnection = battleTank.Tanker.Connection;

        await targetConnection.PurchaseItem(Info.Reward.GetEntity(), Info.Reward.Amount, 0, false, false);

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == targetConnection.Player.Id)
            .Set(stats => stats.GoldBoxesCaught, stats => stats.GoldBoxesCaught + 1)
            .UpdateAsync();
    }

    public override async Task Spawn() {
        Entity = new GoldBonusTemplate().Create(SpawnPosition, RegionEntity.Value, Round.Entity);
        await StateManager.SetState(new Spawned(StateManager));
    }

    public override bool CanBeDropped(bool force) => StateManager.CurrentState is None;

    public async Task Drop(Tanker? tanker) {
        string username = tanker?.Connection.Player.Username ?? "";

        ICollection<BattlePlayer> players = Round.Players;
        await players.Send(new GoldScheduleNotificationEvent(username), Round.Entity);
        await players.Share(RegionEntity.Value);

        await StateManager.SetState(new Cooldown(StateManager, TimeSpan.FromSeconds(20)));
    }

    public override Task Drop() => Drop(null);
}
