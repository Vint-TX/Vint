using Redzen.Random;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Containers;

public class BlueprintsContainer : Container {
    public BlueprintsContainer(IEntity marketItem) : base(marketItem) {
        string configPath = MarketItem.TemplateAccessor!.ConfigPath!;

        Random = new WyRandom();
        Info = ConfigManager.Blueprints[configPath.Split('/').Last()];
        TargetTierItemList = ConfigManager.GetComponent<TargetTierComponent>(configPath).ItemList?
            .Select(id => GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == id))
            .ToList();
    }

    IRandomSource Random { get; }
    BlueprintChest Info { get; }
    List<IEntity>? TargetTierItemList { get; }

    public override IEnumerable<IEntity> Open(IPlayerConnection connection, long amount) {
        Dictionary<IEntity, int> entityToAmount = new();
        int blueprintsAmount = 0;

        for (int i = 0; i < amount; i++)
            blueprintsAmount += Random.Next(Info.MinAmount, Info.MaxAmount);

        if (TargetTierItemList != null) {
            while (blueprintsAmount > 0) {
                AddRandomBlueprint(entityToAmount, TargetTierItemList);
                blueprintsAmount--;
            }
        } else {
            int tier3Amount = 0;
            int tier2Amount = 0;
            int tier1Amount = 0;

            for (int i = 0; i < amount; i++) {
                tier3Amount += Random.NextFloat() < Info.Tier3Probability ? Random.Next(Info.MinTier3Amount, Info.MaxTier3Amount) : 0;
                tier2Amount += Random.NextFloat() < Info.Tier2Probability ? Random.Next(Info.MinTier2Amount, Info.MaxTier2Amount) : 0;
            }

            tier1Amount += Random.NextFloat() < Info.Tier1Probability ? Math.Max(0, blueprintsAmount - (tier3Amount + tier2Amount)) : 0;

            while (blueprintsAmount > 0) {
                IReadOnlyCollection<IEntity> pool;

                if (tier3Amount > 0) {
                    pool = GlobalEntities.Tier3Modules;
                    tier3Amount--;
                } else if (tier2Amount > 0) {
                    pool = GlobalEntities.Tier2Modules;
                    tier2Amount--;
                } else if (tier1Amount > 0) {
                    pool = GlobalEntities.Tier1Modules;
                    tier1Amount--;
                } else break;

                AddRandomBlueprint(entityToAmount, pool);
                blueprintsAmount--;
            }
        }

        return SaveRewards(connection, entityToAmount);
    }

    public IEnumerable<IEntity> SaveRewards(IPlayerConnection connection, Dictionary<IEntity, int> entityToAmount) {
        foreach ((IEntity marketItem, int amount) in entityToAmount.ToList().Shuffle()) {
            connection.PurchaseItem(marketItem, amount, 0, false, false);
            yield return new NewItemNotificationTemplate().CreateCard(MarketItem, marketItem, amount);
        }
    }

    void AddRandomBlueprint(Dictionary<IEntity, int> entityToAmount, IReadOnlyCollection<IEntity> pool) {
        IEntity entity = pool.ElementAt(Random.Next(pool.Count));

        if (!entityToAmount.TryAdd(entity, 1))
            entityToAmount[entity]++;
    }
}