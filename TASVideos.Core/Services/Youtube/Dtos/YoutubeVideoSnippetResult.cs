using Newtonsoft.Json;

namespace TASVideos.Core.Services.Youtube.Dtos;

/// <summary>
/// Represents the snippet portion of a response from a POST/PUT request
/// </summary>
internal class YoutubeVideoSnippetResult
{
	[JsonProperty("title")]
	public string Title { get; init; } = "";

	[JsonProperty("description")]
	public string Description { get; init; } = "";

	[JsonProperty("categoryId")]
	public string CategoryId { get; init; } = "";

	[JsonProperty("tags")]
	public ICollection<string> Tags { get; set; } = new List<string>();
}

/// <summary>
/// Represents the snippet portion of a GET response
/// </summary>
public class YoutubeVideoResponse
{
	[JsonProperty("kind")]
	public string Kind { get; set; } = "";

	[JsonProperty("etag")]
	public string Etag { get; set; } = "";

	public List<YoutubeVideoResponseItem> Items { get; set; } = new();
}

public class YoutubeVideoResponseItem
{
	[JsonProperty("id")]
	public string Id { get; set; } = "";

	[JsonProperty("kind")]
	public string Kind { get; set; } = "";

	[JsonProperty("etag")]
	public string Etag { get; set; } = "";

	[JsonProperty("snippet")]
	public SnippetData Snippet { get; set; } = new();

	public class SnippetData
	{
		[JsonProperty("title")]
		public string Title { get; init; } = "";

		[JsonProperty("description")]
		public string Description { get; init; } = "";

		[JsonProperty("categoryId")]
		public string CategoryId { get; init; } = "";

		[JsonProperty("tags")]
		public ICollection<string> Tags { get; set; } = new List<string>();

		[JsonProperty("channelTitle")]
		public string ChannelTitle { get; set; } = "";
	}
}
