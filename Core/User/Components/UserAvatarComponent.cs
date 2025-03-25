using Vint.Core.Config;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1545809085571)]
public class UserAvatarComponent(string id) : IComponent {
    public UserAvatarComponent(long avatarId) : this(ConfigManager
        .GetComponent<AvatarItemComponent>(
            GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == avatarId).TemplateAccessor?.ConfigPath!).Id) { }

    public string Id { get; } = id;
}
