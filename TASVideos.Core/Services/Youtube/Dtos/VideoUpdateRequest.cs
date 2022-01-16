using Newtonsoft.Json;

namespace TASVideos.Core.Services.Youtube.Dtos
{
	internal class VideoUpdateRequest
	{
		[JsonProperty("kind")]
		public string Kind { get; init; } = "youtube#video";

		[JsonProperty("id")]
		public string VideoId { get; init; } = "";

		[JsonProperty("status")]
		public VideoStatus Status { get; init; } = new();

		[JsonProperty("snippet")]
		public YoutubeVideoSnippetResult Snippet { get; set; } = new ();

		internal class VideoStatus
		{
			[JsonProperty("privacyStatus")]
			public string PrivacyStatus { get; init; } = "public";
		}
	}

	internal class UnlistRequest
	{
		[JsonProperty("kind")]
		public string Kind { get; init; } = "youtube#video";

		[JsonProperty("id")]
		public string VideoId { get; init; } = "";

		[JsonProperty("status")]
		public VideoStatus Status { get; init; } = new();

		internal class VideoStatus
		{
			[JsonProperty("privacyStatus")]
			public string PrivacyStatus { get; init; } = "public";
		}
	}
}
