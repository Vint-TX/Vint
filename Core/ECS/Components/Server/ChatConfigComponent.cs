namespace Vint.Core.ECS.Components.Server;

public class ChatConfigComponent : IComponent {
    public int MaxMessageLength { get; private set; }
}