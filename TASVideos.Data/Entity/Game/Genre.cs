﻿using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity.Game;

[ExcludeFromHistory]
public class Genre
{
	public int Id { get; set; }

	[Required]
	[StringLength(20)]
	public string DisplayName { get; set; } = "";

	public virtual ICollection<GameGenre> GameGenres { get; set; } = new HashSet<GameGenre>();
}
