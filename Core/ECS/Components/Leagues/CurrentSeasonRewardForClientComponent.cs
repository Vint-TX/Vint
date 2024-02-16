using Newtonsoft.Json;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Leagues;

[ProtocolId(1503654626834)]
public class CurrentSeasonRewardForClientComponent(
    int minimumReputation
) : IComponent {
    static CurrentSeasonRewardForClientComponent() {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "leagueRewards.json");

        AllRewards = JsonConvert.DeserializeObject<List<EndSeasonRewardItem>>(File.ReadAllText(path))!;
    }

    [ProtocolIgnore] public static IReadOnlyList<EndSeasonRewardItem> AllRewards { get; }

    public List<EndSeasonRewardItem> Rewards { get; private set; } =
        [..AllRewards.Where(reward => reward.StartPlace <= minimumReputation && reward.EndPlace > minimumReputation)];
}

public class EndSeasonRewardItem(
    long startPlace,
    long endPlace,
    List<DroppedItem>? items
) {
    public long StartPlace { get; private set; } = startPlace;
    public long EndPlace { get; private set; } = endPlace;
    public List<DroppedItem>? Items { get; private set; } = items;
}

public class DroppedItem(
    string marketEntityType,
    string marketEntityName,
    int amount
) {
    IEntity? _marketItemEntity;

    public IEntity MarketItemEntity {
        get {
            _marketItemEntity ??= GlobalEntities.GetEntity(marketEntityType, marketEntityName);
            return _marketItemEntity;
        }
        private set => _marketItemEntity = value;
    }

    public int Amount { get; private set; } = amount;
}