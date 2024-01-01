using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Chat;

[ProtocolId(1513067769958)]
public class PersonalChatOwnerComponent(
    params long[] chatIds
) : IComponent {
    [ProtocolName("ChatIs")] public List<long> ChatIds { get; private set; } = chatIds.ToList();
}