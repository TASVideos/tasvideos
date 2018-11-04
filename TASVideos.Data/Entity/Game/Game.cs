using System.Collections.Generic;
using System.ComponentModel;

namespace TASVideos.Data.Entity.Game
{
	/// <summary>
	/// Represents a Game
	/// This is the central reference point for all site content
	/// </summary>
	public class Game : BaseEntity
	{
		public int Id { get; set; }
		public virtual ICollection<GameRom> Roms { get; set; } = new HashSet<GameRom>();

		public int SystemId { get; set; }
		public virtual GameSystem System { get; set; }

		public virtual ICollection<Publication> Publications { get; set; } = new HashSet<Publication>();
		public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
		public virtual ICollection<GameGenre> GameGenres { get; set; } = new HashSet<GameGenre>();
		public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();

		[Description("Good Set or some other official naming convention")]
		public string GoodName { get; set; }

		public string DisplayName { get; set; }

		public string Abbreviation { get; set; }

		public string SearchKey { get; set; }

		public string YoutubeTags { get; set; }

		public string ScreenshotUrl { get; set; }
	}
}
