using Vint.Core.Battle.Graffiti.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Graffiti.Templates;

[ProtocolId(636100801926133320)]
public class GraffitiBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity graffiti, IEntity tank) => Entity(graffiti.TemplateAccessor!.ConfigPath,
        builder => builder
            .AddComponent<GraffitiBattleItemComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddGroupComponent<MarketItemGroupComponent>(graffiti));
}
