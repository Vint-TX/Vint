using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Modules;

[ProtocolId(1484901449548)]
public class ModuleUserItemTemplate : EntityTemplate {
    public IEntity Create(IEntity tank, IEntity userItem) =>
        Entity(userItem.TemplateAccessor?.ConfigPath,
            builder => builder
                .AddComponent<ModuleUsesCounterComponent>()
                .AddGroupComponent<TankGroupComponent>(tank));
}
