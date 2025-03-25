namespace Vint.Core.News;

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
}
