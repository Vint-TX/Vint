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

            if (ItemsComponent.RareItems == null || ItemsComponent.RareItems.Count == 0 || random.NextFloat() > 0.05) continue;

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
        int rollCountLeft = 10;
        IEntity reward;

        do {
            ContainerItem item = rewards[random.Next(rewards.Count)];
            MarketItemBundle bundle = item.ItemBundles[random.Next(item.ItemBundles.Count)];

            compensation = item.Compensation;
            amount = random.Next(bundle.Amount, bundle.Max + 1);
            reward = connection.SharedEntities.Single(entity => entity.Id == bundle.MarketItem);
            rollCountLeft--;
        } while (rollCountLeft >= 0 && !ValidateReward(connection, reward));

        if (rollCountLeft >= 0 || ValidateReward(connection, reward)) return reward;

        reward = GlobalEntities.GetEntity("misc", "Crystal");
        amount = (int)compensation;
        return reward;
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

    static bool ValidateReward(IPlayerConnection connection, IEntity marketItem) {
        if (!marketItem.HasComponent<ParentGroupComponent>()) return true;

        EntityTemplate? template = marketItem.TemplateAccessor?.Template;

        if (template == null) return false;

        return template is not
                   (HullSkinMarketItemTemplate or WeaponSkinMarketItemTemplate or ShellMarketItemTemplate) ||
               connection.OwnsItem(GlobalEntities.AllMarketTemplateEntities
                   .Single(entity =>
                       entity.Id == marketItem.GetComponent<ParentGroupComponent>().Key));
    }
}