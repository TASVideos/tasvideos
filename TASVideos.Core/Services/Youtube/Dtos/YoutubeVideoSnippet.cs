using Newtonsoft.Json;

namespace TASVideos.Core.Services.Youtube.Dtos
{
	internal class YoutubeVideoSnippet
	{
		[JsonProperty("title")]
		public string Title { get; init; } = "";

		[JsonProperty("description")]
		public string Description { get; init; } = "";

		[JsonProperty("categoryId")]
		public string CategoryId { get; init; } = "";
	}
}
