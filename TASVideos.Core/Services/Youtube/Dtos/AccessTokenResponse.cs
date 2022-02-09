using Newtonsoft.Json;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class AccessTokenResponse
{
	[JsonProperty("access_token")]
	public string AccessToken { get; init; } = "";

	[JsonProperty("expires_in")]
	public int ExpiresAt { get; init; }

	[JsonProperty("scope")]
	public string Scope { get; init; } = "";
}
