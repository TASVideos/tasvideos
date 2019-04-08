using System.Collections.Generic;
using TASVideos.Data.Entity.Game;

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

		public ICollection<Rom> Roms { get; set; } = new List<Rom>();
		public class Rom
		{
			public RomTypes Type { get; set; }
			public int Id { get; set; }
			public string Md5 { get; set; }
			public string Sha1 { get; set; }
			public string Name { get; set; }
			public string Region { get; set; }
			public string Version { get; set; }
		}
	}
}
