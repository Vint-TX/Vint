using System.Numerics;
using Vint.Core.ECS.Components.Battle.Bonus;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Bonus;

[ProtocolId(-8402520789413485183)]
public class GoldBonusTemplate : BonusTemplate {
    public IEntity Create(Vector3 position, IEntity regionEntity, IEntity battleEntity) {
        IEntity entity = Create("battle/bonus/gold/cry", position, regionEntity, battleEntity);

        entity.AddComponentFrom<GoldBonusRegionComponent>(regionEntity);
        return entity;
    }
}