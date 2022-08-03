﻿using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class SubmissionAuthor
{
	public int UserId { get; set; }
	public virtual User? Author { get; set; }

	[Required]
	public int Ordinal { get; set; }

	public int SubmissionId { get; set; }
	public virtual Submission? Submission { get; set; }
}
