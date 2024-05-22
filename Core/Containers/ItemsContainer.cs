using Redzen.Random;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.ECS.Templates.Shells;
using Vint.Core.ECS.Templates.Skins;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Containers;

public class ItemsContainer(
    IEntity marketItem
) : Container(marketItem) {
    ItemsContainerItemComponent ItemsComponent { get; } =
        ConfigManager.GetComponent<ItemsContainerItemComponent>(marketItem.TemplateAccessor!.ConfigPath!);

    public override async IAsyncEnumerable<IEntity> Open(IPlayerConnection connection, long amount) {
        WyRandom random = new();

        while (amount > 0) {
            amount--;

            (IEntity regularReward, int itemAmount, long compensation) = await GetReward(ItemsComponent.Items, connection, random);
            yield return await SaveRewardOrCompensation(connection, regularReward, itemAmount, compensation);

            if (ItemsComponent.RareItems == null || ItemsComponent.RareItems.Count == 0 || !MathUtils.RollTheDice(0.1, random) /* 10% */) continue;

            (IEntity rareReward, itemAmount, compensation) = await GetReward(ItemsComponent.RareItems, connection, random);
            yield return await SaveRewardOrCompensation(connection, rareReward, itemAmount, compensation);
        }
    }

    static async Task<(IEntity, int, long)> GetReward(
        IReadOnlyList<ContainerItem> rewards,
        IPlayerConnection connection,
        IRandomSource random) {
        int rollCountLeft = 10;
        IEntity reward;
        int amount;
        long compensation;

        do {
            ContainerItem item = rewards[random.Next(rewards.Count)];
            MarketItemBundle bundle = item.ItemBundles[random.Next(item.ItemBundles.Count)];

            compensation = item.Compensation;
            amount = random.Next(bundle.Amount, bundle.Max + 1);
            reward = connection.SharedEntities.Single(entity => entity.Id == bundle.MarketItem);
            rollCountLeft--;
        } while (rollCountLeft >= 0 && !await ValidateReward(connection, reward));

        return rollCountLeft >= 0 || await ValidateReward(connection, reward)
            ? (reward, amount, compensation)
            : (GlobalEntities.GetEntity("misc", "Crystal"), (int)compensation, compensation);
    }

    async Task<IEntity> SaveRewardOrCompensation(IPlayerConnection connection, IEntity marketItem, int amount, long compensation) {
        if (await connection.OwnsItem(marketItem)) {
            marketItem = GlobalEntities.GetEntity("misc", "Crystal");
            amount = (int)compensation;
            await connection.ChangeCrystals(compensation);
        } else
            await connection.PurchaseItem(marketItem, amount, 0, false, false);

        return new NewItemNotificationTemplate().CreateRegular(MarketItem, marketItem, amount);
    }

    static async Task<bool> ValidateReward(IPlayerConnection connection, IEntity marketItem) {
        if (!marketItem.HasComponent<ParentGroupComponent>()) return true;

        EntityTemplate? template = marketItem.TemplateAccessor?.Template;

        if (template == null) return false;

        return template is not (
                   HullSkinMarketItemTemplate or
                   WeaponSkinMarketItemTemplate or
                   ShellMarketItemTemplate) ||
               await connection.OwnsItem(GlobalEntities.AllMarketTemplateEntities.Single(entity =>
                   entity.Id == marketItem.GetComponent<ParentGroupComponent>().Key));
    }
}
