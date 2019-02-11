using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Game
{
	public class Genre
	{
		public int Id { get; set; }

		[Required]
		[StringLength(20)]
		public string DisplayName { get; set; }

		public virtual ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
	}
}