﻿using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace TASVideos.Data.Entity.Forum;

[ExcludeFromHistory]
public class ForumPost : BaseEntity
{
	public int Id { get; set; }

	public int? TopicId { get; set; }
	public virtual ForumTopic? Topic { get; set; }

	public int ForumId { get; set; }
	public virtual Forum? Forum { get; set; }

	public int PosterId { get; set; }
	public virtual User? Poster { get; set; }

	[StringLength(50)]
	public string? IpAddress { get; set; }

	[StringLength(500)]
	public string? Subject { get; set; }

	[Required]
	public string Text { get; set; } = "";

	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }

	public ForumPostMood PosterMood { get; set; }

	[JsonIgnore]
	public NpgsqlTsVector SearchVector { get; set; } = null!;
}

public static class ForumPostQueryableExtensions
{
	public static IQueryable<ForumPost> ExcludeRestricted(this IQueryable<ForumPost> list, bool seeRestricted)
	{
		return list.Where(f => seeRestricted || !f.Topic!.Forum!.Restricted);
	}

	public static IQueryable<ForumPost> ForTopic(this IQueryable<ForumPost> list, int topicId)
	{
		return list.Where(p => p.TopicId == topicId);
	}
}
