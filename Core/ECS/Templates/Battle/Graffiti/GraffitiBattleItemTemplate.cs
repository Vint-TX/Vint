using Vint.Core.ECS.Components.Battle.Graffiti;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Graffiti;

[ProtocolId(636100801926133320)]
public class GraffitiBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity graffiti, IEntity tank) => Entity(graffiti.TemplateAccessor!.ConfigPath,
        builder =>
            builder
                .AddComponent<GraffitiBattleItemComponent>()
                .AddComponentFrom<UserGroupComponent>(tank)
                .AddComponentFrom<MarketItemGroupComponent>(graffiti));
}