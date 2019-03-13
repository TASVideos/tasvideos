using System.Collections.Generic;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;

#pragma warning disable 1591
namespace TASVideos.Api.Responses
{
	/// <summary>
	/// Represents a game returned by the games endpoint
	/// </summary>
	public class GamesResponse
	{
		[Sortable]
		public int Id { get; set; }

		public IEnumerable<GameRom> Roms { get; set; } = new List<GameRom>();

		[Sortable]
		public string SystemCode { get; set; }

		[Sortable]
		public string GoodName { get; set; }

		[Sortable]
		public string DisplayName { get; set; }

		[Sortable]
		public string Abbreviation { get; set; }

		[Sortable]
		public string SearchKey { get; set; }

		[Sortable]
		public string YoutubeTags { get; set; }

		[Sortable]
		public string ScreenshotUrl { get; set; }

		public class GameRom
		{
			public int Id { get; set; }
			public string Md5 { get; set; }
			public string Sha1 { get; set; }
			public string Name { get; set; }
			public RomTypes Type { get; set; }
			public string Region { get; set; }
			public string Version { get; set; }
		}
	}
}
