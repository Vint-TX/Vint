using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.News.Components;

[ProtocolId(1479374709878)]
public class NewsItemComponent(
    NewsItem newsItem
) : IComponent {
    public NewsItem NewsItem { get; private set; } = newsItem;
}
