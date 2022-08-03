﻿using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity.Forum;

[ExcludeFromHistory]
public class ForumPollOptionVote
{
	public int Id { get; set; }

	public int PollOptionId { get; set; }
	public virtual ForumPollOption? PollOption { get; set; }

	public int UserId { get; set; }
	public virtual User? User { get; set; }

	public DateTime CreateTimestamp { get; set; }

	[StringLength(50)]
	public string? IpAddress { get; set; }
}
