using System.Collections.Frozen;
using Vint.Core.Battle.Bonus.Type;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Bonus;

public interface IBonusProcessor {
    GoldProcessor GoldProcessor { get; }

    Task Init();

    Task OnWarmUpEnded();

    Task Tick(TimeSpan deltaTime);

    Task Take(BonusBox bonus, BattleTank tank);

    Task ShareEntities(IPlayerConnection connection);

    Task UnshareEntities(IPlayerConnection connection);

    BonusBox? FindByEntity(IEntity bonusEntity);

    Task<bool> ForceDropBonus(BonusType type, Tanker? tanker);
}

public class BonusProcessor : IBonusProcessor {
    public BonusProcessor(Round round) {
        BattleProperties properties = round.Properties;
        Dictionary<BonusType, IEnumerable<BonusInfo>> bonusInfos = properties.MapInfo.BonusRegions
            .Get(properties.BattleMode)
            .ToDictionary();

        List<BonusBox> bonuses = [];

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach ((BonusType type, IEnumerable<BonusInfo> bonusesInfo) in bonusInfos) {
            IEnumerable<BonusBox> bonusBoxes = type switch {
                BonusType.Repair => bonusesInfo.Select(bonusInfo => new RepairBox(round, bonusInfo)),
                BonusType.Armor => bonusesInfo.Select(bonusInfo => new ArmorBox(round, bonusInfo)),
                BonusType.Damage => bonusesInfo.Select(bonusInfo => new DamageBox(round, bonusInfo)),
                BonusType.Speed => bonusesInfo.Select(bonusInfo => new SpeedBox(round, bonusInfo)),
                BonusType.Gold => bonusesInfo.Select(bonusInfo => new GoldBox(round, bonusInfo)),
                _ => throw new InvalidOperationException($"Unexpected bonus type {type}")
            };

            bonuses.AddRange(bonusBoxes);
        }

        Bonuses = bonuses.Shuffle().ToFrozenSet();
        GoldProcessor = new GoldProcessor(round, Bonuses.OfType<GoldBox>().ToArray(), ConfigManager.CommonMapInfo.Gold);
    }

    FrozenSet<BonusBox> Bonuses { get; }

    public GoldProcessor GoldProcessor { get; }

    public async Task Init() {
        foreach (BonusBox bonus in Bonuses) {
            await bonus.Init();

            if (bonus is not SupplyBox supply)
                continue;

            await supply.StateManager.SetState(GetInitIdleState(supply));
        }
    }

    public async Task OnWarmUpEnded() =>
        await ResetBonusesState();

    public async Task Tick(TimeSpan deltaTime) {
        foreach (BonusBox bonus in Bonuses)
            await bonus.Tick(deltaTime);
    }

    public async Task Take(BonusBox bonus, BattleTank tank) =>
        await bonus.Take(tank);

    public async Task ShareEntities(IPlayerConnection connection) {
        List<IEntity> entities = new(Bonuses.Count * 2);

        foreach (BonusBox bonus in Bonuses.Where(bonus => bonus is not GoldBox || bonus.StateManager.CurrentState is not None)) {
            entities.Add(bonus.RegionEntity.Value);

            if (bonus.Entity != null)
                entities.Add(bonus.Entity);
        }

        await connection.ShareIfUnshared(entities);
    }

    public async Task UnshareEntities(IPlayerConnection connection) {
        List<IEntity> entities = new(Bonuses.Count * 2);

        foreach (BonusBox bonus in Bonuses) {
            entities.Add(bonus.RegionEntity.Value);

            if (bonus.Entity != null)
                entities.Add(bonus.Entity);
        }

        await connection.UnshareIfShared(entities);
    }

    public BonusBox? FindByEntity(IEntity bonusEntity) =>
        Bonuses.FirstOrDefault(bonus => bonus.Entity == bonusEntity);

    public async Task<bool> ForceDropBonus(BonusType type, Tanker? tanker) {
        if (type == BonusType.Gold)
            return await GoldProcessor.DropRandom(tanker, true);

        BonusBox? bonus = Bonuses
            .ToArray()
            .Shuffle()
            .FirstOrDefault(bonus => bonus.Type == type && bonus.CanBeDropped(true));

        if (bonus == null)
            return false;

        await bonus.Drop();
        return true;
    }

    async Task ResetBonusesState() {
        foreach (BonusBox bonus in Bonuses) {
            if (await bonus.TryDestroy())
                await bonus.StateManager.SetState(GetInitIdleState(bonus));
        }
    }

    static BonusState GetInitIdleState(BonusBox box) => box switch {
        SupplyBox => new Cooldown(box.StateManager, GetRandomCooldown()),
        GoldBox => new None(box.StateManager),
        _ => throw new InvalidOperationException($"Unexpected bonus type {box.Type}")
    };

    static TimeSpan GetRandomCooldown() =>
        TimeSpan.FromSeconds(Random.Shared.Next(60));
}
