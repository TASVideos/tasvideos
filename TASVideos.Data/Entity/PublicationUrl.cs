﻿using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

public enum PublicationUrlType { Streaming, Mirror }

[IncludeInAutoHistory]
public class PublicationUrl : BaseEntity
{
	public int Id { get; set; }
	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }

	[Required]
	public string? Url { get; set; }

	public PublicationUrlType Type { get; set; } = PublicationUrlType.Streaming;

	public string? DisplayName { get; set; }
}

public static class PublicationUrlExtensions
{
	public static void AddMirror(this ICollection<PublicationUrl> urls, string url)
	{
		urls.Add(new PublicationUrl
		{
			Url = url,
			Type = PublicationUrlType.Mirror
		});
	}

	public static void AddStreaming(this ICollection<PublicationUrl> urls, string url, string? displayName)
	{
		urls.Add(new PublicationUrl
		{
			Url = url,
			DisplayName = displayName.NullIfWhitespace(),
			Type = PublicationUrlType.Streaming
		});
	}

	public static IQueryable<PublicationUrl> ThatAreStreaming(this IQueryable<PublicationUrl> urls)
	{
		return urls.Where(u => u.Type == PublicationUrlType.Streaming);
	}

	public static IEnumerable<PublicationUrl> ThatAreStreaming(this IEnumerable<PublicationUrl> urls)
	{
		return urls.Where(u => u.Type == PublicationUrlType.Streaming);
	}
}
