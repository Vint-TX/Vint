using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1479198715562)]
public class RestorePasswordCodeSentComponent(
    string discordUsername
) : IComponent {
    [ProtocolName("Email")] public string DiscordUsername { get; private set; } = discordUsername;
}
