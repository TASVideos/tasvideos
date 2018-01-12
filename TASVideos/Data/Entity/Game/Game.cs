using System.Collections.Generic;
using System.ComponentModel;

namespace TASVideos.Data.Entity.Game
{
	/// <summary>
	/// Represents a Game
	/// This is the central reference point for all site content
	/// </summary>
	public class Game
	{
		public int Id { get; set; }
		public virtual ICollection<GameRom> Roms { get; set; } = new HashSet<GameRom>();

		[Description("Good Set or some other official naming convention")]
		public string GoodName { get; set; }

		public string DisplayName { get; set; }

		public string Abbreviation { get; set; }

		public int SystemId { get; set; }
		public virtual GameSystem System { get; set; }

		public string SearchKey { get; set; }

		public string YoutubeTags { get; set; }

		public string ScreenshotUrl { get; set; }
	}
}
