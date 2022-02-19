using System.Text.Json.Serialization;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class AccessTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; init; } = "";

	[JsonPropertyName("expires_in")]
	public int ExpiresAt { get; init; }

	[JsonPropertyName("scope")]
	public string Scope { get; init; } = "";
}
