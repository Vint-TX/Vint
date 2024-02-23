using Redzen.Random;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server;

namespace Vint.Core.Containers;

public class ItemsContainer(
    IEntity marketItem
) : Container(marketItem) {
    ItemsContainerItemComponent ItemsComponent { get; } =
        ConfigManager.GetComponent<ItemsContainerItemComponent>(marketItem.TemplateAccessor!.ConfigPath!);

    public override IEnumerable<IEntity> Open(IPlayerConnection connection, long amount) {
        WyRandom random = new();

        while (amount > 0) {
            amount--;

            IEntity regularReward = GetReward(ItemsComponent.Items, connection, random, out int itemAmount, out long compensation);
            yield return SaveRewardOrCompensation(connection, regularReward, itemAmount, compensation);

            if (ItemsComponent.RareItems.Count == 0 || random.NextFloat() > 0.05) continue;

            IEntity rareReward = GetReward(ItemsComponent.RareItems, connection, random, out itemAmount, out compensation);
            yield return SaveRewardOrCompensation(connection, rareReward, itemAmount, compensation);
        }
    }

    static IEntity GetReward(
        IReadOnlyList<ContainerItem> rewards,
        IPlayerConnection connection,
        IRandomSource random,
        out int amount,
        out long compensation) {
        ContainerItem item = rewards[random.Next(rewards.Count)];
        MarketItemBundle bundle = item.ItemBundles[random.Next(item.ItemBundles.Count)];

        compensation = item.Compensation;
        amount = random.Next(bundle.Amount, bundle.Max + 1);
        return connection.SharedEntities.Single(entity => entity.Id == bundle.MarketItem);
    }

    IEntity SaveRewardOrCompensation(IPlayerConnection connection, IEntity marketItem, int amount, long compensation) {
        if (connection.OwnsItem(marketItem)) {
            marketItem = GlobalEntities.GetEntity("misc", "Crystal");
            amount = (int)compensation;
            connection.SetCrystals(connection.Player.Crystals + compensation);
        } else
            connection.PurchaseItem(marketItem, amount, 0, false, false);

        return new NewItemNotificationTemplate().Create(MarketItem, marketItem, amount);
    }
}