using System.Collections.Generic;

namespace TASVideos.Pages.Games.Models
{
	public class GameDisplayModel
	{
		public int Id { get; set; }
		public string DisplayName { get; set; }
		public string Abbreviation { get; set; }
		public string ScreenshotUrl { get; set; }
		public string SystemCode { get; set; }
		public string GoodName { get; set; }
		public string GameResourcesPage { get; set; }
		public IEnumerable<string> Genres { get; set; } = new List<string>();
	}
}
