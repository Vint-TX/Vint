using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.ClientSession.Components;

[ProtocolId(1479198715562)]
public class RestorePasswordCodeSentComponent(
    string discordUsername
) : IComponent {
    public string DiscordUsername => discordUsername;
}
