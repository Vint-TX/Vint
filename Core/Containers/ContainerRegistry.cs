using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Containers;

namespace Vint.Core.Containers;

public static class ContainerRegistry {
    public static Container GetContainer(IEntity marketItem) => marketItem.TemplateAccessor!.Template switch {
        ContainerPackPriceMarketItemTemplate => new ItemsContainer(marketItem),
        DonutChestMarketItemTemplate => new BlueprintsContainer(marketItem),
        GameplayChestMarketItemTemplate => new BlueprintsContainer(marketItem),
        TutorialGameplayChestMarketItemTemplate => new BlueprintsContainer(marketItem),
        _ => throw new ArgumentException(null, nameof(marketItem))
    };
}