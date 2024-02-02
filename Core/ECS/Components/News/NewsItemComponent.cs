using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.News;

[ProtocolId(1479374709878)]
public class NewsItemComponent(
    NewsItem newsItem
) : IComponent {
    public NewsItem NewsItem { get; private set; } = newsItem;
}

public class NewsItem(
    NewsItemLayout layout,
    bool previewImageFitInParent = false,
    string? headerText = null,
    string? previewImageUrl = null,
    string? previewImageGuid = null,
    string? centralIconGuid = null,
    string? tooltip = null,
    string? externalUrl = null,
    string? internalUrl = null,
    DateTimeOffset? date = null
) {
    public bool PreviewImageFitInParent { get; set; } = previewImageFitInParent;
    public string? HeaderText { get; set; } = headerText;
    public string? PreviewImageUrl { get; set; } = previewImageUrl;
    public string? PreviewImageGuid { get; set; } = previewImageGuid;
    public string? CentralIconGuid { get; set; } = centralIconGuid;
    public string? Tooltip { get; set; } = tooltip;
    public string? ExternalUrl { get; set; } = externalUrl;
    public string? InternalUrl { get; set; } = internalUrl;
    public DateTimeOffset? Date { get; set; } = date;
    public NewsItemLayout Layout { get; set; } = layout;
    public string? ShortText => null; // not used
    public string? LongText => null; // not used
}