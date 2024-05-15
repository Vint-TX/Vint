using System.Text.Json.Serialization;

namespace Vint.Core.Discord;

public class OAuth2Data {
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = null!;
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; init; } = null!;
    [JsonPropertyName("token_type")] public string TokenType { get; init; } = null!;
    [JsonPropertyName("scope")] public string Scope { get; init; } = null!;
}
