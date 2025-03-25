using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1439270018242)]
public class RegistrationDateComponent(
    DateTimeOffset? date
) : PrivateComponent {
    public DateTimeOffset? Date { get; private set; } = date;
}
