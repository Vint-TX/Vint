using Vint.Core.ECS.Entities;

namespace Vint.Core.Config.MapInformation;

public readonly record struct GoldMapInfo(
    double Probability,
    Reward Reward
);

public record struct Reward(
    long Id,
    int Amount
) {
    IEntity? _entity;

    public IEntity GetEntity() {
        if (_entity != null)
            return _entity;

        long id = Id;
        return _entity = GlobalEntities.AllMarketTemplateEntities.First(entity => entity.Id == id);
    }
}
