using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Avatar;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1545809085571)]
public class UserAvatarComponent(
    IPlayerConnection connection,
    long avatarId
) : IComponent {
    public string Id { get; set; } = ConfigManager.GetComponent<AvatarItemComponent>(connection.GetEntity(avatarId)
            ?.TemplateAccessor?.ConfigPath!)
        .Id;
}
