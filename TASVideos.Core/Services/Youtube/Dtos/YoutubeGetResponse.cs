using Newtonsoft.Json;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class YoutubeGetResponse
{
	[JsonProperty("items")]
	public ICollection<Item> Items { get; set; } = new List<Item>();

	public class Item
	{
		[JsonProperty("snippet")]
		public YoutubeVideoSnippetResult Snippet { get; set; } = new();
	}
}
