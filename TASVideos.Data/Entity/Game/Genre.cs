using System.Collections.Generic;

namespace TASVideos.Data.Entity.Game
{
	public class Genre
	{
		public int Id { get; set; }
		public string DisplayName { get; set; }

		public virtual ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
	}
}