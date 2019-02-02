using System.Collections.Generic;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a <see cref="TASVideos.Data.Entity.Game.Game"/> for the purpose of displaying
	/// on a dedicated page
	/// </summary>
	public class GameDisplayModel
	{
		public string DisplayName { get; set; }
		public string Abbreviation { get; set; }
		public string ScreenshotUrl { get; set; }
		public string SystemCode { get; set; }

		public IEnumerable<string> Genres { get; set; } = new List<string>();
	}
}
