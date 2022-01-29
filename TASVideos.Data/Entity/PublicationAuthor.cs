﻿using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity;

public class PublicationAuthor
{
	public int UserId { get; set; }
	public virtual User? Author { get; set; }

	[Required]
	public int Ordinal { get; set; }

	public int PublicationId { get; set; }
	public virtual Publication? Publication { get; set; }
}
