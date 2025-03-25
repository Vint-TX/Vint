using Vint.Core.ECS.Components;

namespace Vint.Core.Chat.Components;

public class ChatConfigComponent : IComponent {
    public int MaxMessageLength { get; private set; }
}
