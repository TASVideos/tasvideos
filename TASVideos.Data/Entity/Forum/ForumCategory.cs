﻿using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity.Forum;

[ExcludeFromHistory]
public class ForumCategory : BaseEntity
{
	public int Id { get; set; }
	public virtual ICollection<Forum> Forums { get; set; } = new HashSet<Forum>();

	[Required]
	[StringLength(30)]
	public string Title { get; set; } = "";

	public int Ordinal { get; set; }

	[StringLength(1000)]
	public string? Description { get; set; }
}
