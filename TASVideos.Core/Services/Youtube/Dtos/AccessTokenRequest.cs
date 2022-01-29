using Newtonsoft.Json;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class AccessTokenRequest
{
	[JsonProperty("grant_type")]
	public string GrantType { get; init; } = "refresh_token";

	[JsonProperty("client_id")]
	public string ClientId { get; init; } = "";

	[JsonProperty("client_secret")]
	public string ClientSecret { get; init; } = "";

	[JsonProperty("refresh_token")]
	public string RefreshToken { get; init; } = "";
}
