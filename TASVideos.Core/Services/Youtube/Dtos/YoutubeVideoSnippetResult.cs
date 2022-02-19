using System.Text.Json.Serialization;

namespace TASVideos.Core.Services.Youtube.Dtos;

/// <summary>
/// Represents the snippet portion of a response from a POST/PUT request
/// </summary>
internal class YoutubeVideoSnippetResult
{
	[JsonPropertyName("title")]
	public string Title { get; init; } = "";

	[JsonPropertyName("description")]
	public string Description { get; init; } = "";

	[JsonPropertyName("categoryId")]
	public string CategoryId { get; init; } = "";

	[JsonPropertyName("tags")]
	public ICollection<string> Tags { get; set; } = new List<string>();
}

/// <summary>
/// Represents the snippet portion of a GET response
/// </summary>
public class YoutubeVideoResponse
{
	[JsonPropertyName("kind")]
	public string Kind { get; set; } = "";

	[JsonPropertyName("etag")]
	public string Etag { get; set; } = "";

	public List<YoutubeVideoResponseItem> Items { get; set; } = new();
}

public class YoutubeVideoResponseItem
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";

	[JsonPropertyName("kind")]
	public string Kind { get; set; } = "";

	[JsonPropertyName("etag")]
	public string Etag { get; set; } = "";

	[JsonPropertyName("snippet")]
	public SnippetData Snippet { get; set; } = new();

	public class SnippetData
	{
		[JsonPropertyName("title")]
		public string Title { get; init; } = "";

		[JsonPropertyName("description")]
		public string Description { get; init; } = "";

		[JsonPropertyName("categoryId")]
		public string CategoryId { get; init; } = "";

		[JsonPropertyName("tags")]
		public ICollection<string> Tags { get; set; } = new List<string>();

		[JsonPropertyName("channelTitle")]
		public string ChannelTitle { get; set; } = "";
	}
}
