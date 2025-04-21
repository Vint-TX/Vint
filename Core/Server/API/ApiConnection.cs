using EmbedIO.WebSockets;

namespace Vint.Core.Server.API;

public class ApiConnection(
    IWebSocketContext context
) {
    public IWebSocketContext Context { get; } = context;
    public Subscriptions Subscriptions { get; set; } = Subscriptions.None;
}
