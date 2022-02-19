using System.Text.Json.Serialization;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class VideoUpdateRequest
{
	[JsonPropertyName("kind")]
	public string Kind { get; init; } = "youtube#video";

	[JsonPropertyName("id")]
	public string VideoId { get; init; } = "";

	[JsonPropertyName("status")]
	public VideoStatus Status { get; init; } = new();

	[JsonPropertyName("snippet")]
	public YoutubeVideoSnippetResult Snippet { get; set; } = new();

	internal class VideoStatus
	{
		[JsonPropertyName("privacyStatus")]
		public string PrivacyStatus { get; init; } = "public";

		[JsonPropertyName("license")]
		public string License { get; init; } = "youtube";

		[JsonPropertyName("embeddable")]
		public bool Embeddable { get; init; } = true;

		[JsonPropertyName("publicStatsViewable")]
		public bool PublicStatsViewable { get; init; }

		[JsonPropertyName("madeForKids")]
		public bool MadeForKids { get; init; }
	}
}

internal class UnlistRequest
{
	[JsonPropertyName("kind")]
	public string Kind { get; init; } = "youtube#video";

	[JsonPropertyName("id")]
	public string VideoId { get; init; } = "";

	[JsonPropertyName("status")]
	public VideoStatus Status { get; init; } = new();

	internal class VideoStatus
	{
		[JsonPropertyName("privacyStatus")]
		public string PrivacyStatus { get; init; } = "public";

		[JsonPropertyName("license")]
		public string License { get; init; } = "youtube";

		[JsonPropertyName("embeddable")]
		public bool Embeddable { get; init; } = true;

		[JsonPropertyName("publicStatsViewable")]
		public bool PublicStatsViewable { get; init; }

		[JsonPropertyName("madeForKids")]
		public bool MadeForKids { get; init; }
	}
}
