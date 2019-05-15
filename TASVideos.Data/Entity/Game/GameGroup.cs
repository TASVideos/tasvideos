using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Game
{
	public class GameGroup
	{
		public int Id { get; set; }

		// todo unique constraint
		[Required]
		[StringLength(255)]
		public string Name { get; set; }

		[Required]
		[StringLength(255)]
		public string SearchKey { get; set; }

		public ICollection<GameGameGroup> Games { get; set; } = new HashSet<GameGameGroup>();
	}
}
