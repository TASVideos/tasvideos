using System.Text.Json.Serialization;

namespace TASVideos.Core.Services.Youtube.Dtos;

internal class YoutubeGetResponse
{
	[JsonPropertyName("items")]
	public ICollection<Item> Items { get; set; } = new List<Item>();

	public class Item
	{
		[JsonPropertyName("snippet")]
		public YoutubeVideoSnippetResult Snippet { get; set; } = new();
	}
}
