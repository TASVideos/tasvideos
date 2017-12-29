using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Game
{
	/// <summary>
	/// Represents the system that a game runs on, such as NES, SNES, Commodore 64, PSX, etc
	/// </summary>
	public class GameSystem
    {
		public int Id { get; set; } // Note that this is Non-auto-incrementing, we need Ids to be identical across any database

		[Required]
		[StringLength(8)]
		public string Code { get; set; }

		[Required]
		[StringLength(100)]
		public string DisplayName { get; set; }
	}
}
