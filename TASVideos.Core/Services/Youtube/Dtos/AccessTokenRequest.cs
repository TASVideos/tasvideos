using System.Text.Json.Serialization;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class AccessTokenRequest
{
	[JsonPropertyName("grant_type")]
	public string GrantType { get; init; } = "refresh_token";

	[JsonPropertyName("client_id")]
	public string ClientId { get; init; } = "";

	[JsonPropertyName("client_secret")]
	public string ClientSecret { get; init; } = "";

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; init; } = "";
}
