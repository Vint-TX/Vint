using System.Numerics;
using Vint.Core.Battle.Bonus.Components;
using Vint.Core.Battle.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Templates;

[ProtocolId(7553964914512142106)]
public abstract class BonusTemplate : EntityTemplate {
    protected IEntity Create(string configPath, Vector3 position, IEntity regionEntity, IEntity roundEntity) => Entity(configPath,
        builder => builder
            .AddComponent<BonusComponent>()
            .AddComponent(new RotationComponent(default))
            .AddComponent(new PositionComponent(position))
            .AddComponent(new BonusDropTimeComponent(DateTimeOffset.UtcNow))
            .AddGroupComponent<BattleGroupComponent>(roundEntity)
            .AddGroupComponent<BonusRegionGroupComponent>(regionEntity));
}
