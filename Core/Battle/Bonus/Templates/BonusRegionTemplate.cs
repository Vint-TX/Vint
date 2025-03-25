using System.Numerics;
using Vint.Core.Battle.Bonus.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Templates;

[ProtocolId(8116072916726390829)]
public class BonusRegionTemplate : EntityTemplate {
    public IEntity CreateRegular(BonusType type, Vector3 position) {
        IEntity entity = Create(type, position);

        entity.AddComponent<SupplyBonusRegionComponent>();
        return entity;
    }

    public IEntity CreateGold(Vector3 position) {
        IEntity entity = Create(BonusType.Gold, position);

        entity.AddComponent<GoldBonusRegionComponent>();
        entity.AddComponent<BonusComponent>();
        return entity;
    }

    IEntity Create(BonusType type, Vector3 position) => Entity("battle/bonus/region",
        builder => builder
            .AddComponent(new BonusRegionComponent(type))
            .AddComponent(new SpatialGeometryComponent(position, default))
            .AddGroupComponent<BonusRegionGroupComponent>());
}
