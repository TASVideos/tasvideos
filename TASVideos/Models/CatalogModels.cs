using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Models
{
	public class GameListRequest : PagedModel
	{
		public GameListRequest()
		{
			PageSize = 25;
		}

		public string SystemCode { get; set; }
	}

	public class GameListModel
    {
		[Sortable]
		[Display(Name = "Id")]
		public int Id { get; set; }

		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Sortable]
		[Display(Name = "Name")]
		public string DisplayName { get; set; }

		// Dummy to generate column header
		[Display(Name = "Actions")]
		public object Actions { get; set; }
	}

	public class GameEditModel
	{
		public int? Id { get; set; }
		public bool CanDelete { get; set; }

		[Required]
		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Required]
		[Display(Name = "Good Name")]
		public string GoodName { get; set; }

		[Required]
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		[Required]
		[Display(Name = "Abbreviation")]
		public string Abbreviation { get; set; }

		[Required]
		[Display(Name = "Search Key")]
		public string SearchKey { get; set; }

		[Required]
		[Display(Name = "Youtube Tags")]
		public string YoutubeTags { get; set; }

		[Display(Name = "Screenshot Url")]
		public string ScreenshotUrl { get; set; }

		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
	}

	public class RomListModel
	{
		public int GameId { get; set; }

		[Display(Name = "Game")]
		public string GameDisplayName { get; set; }

		[Display(Name = "System")]
		public string SystemCode { get; set; }

		public IEnumerable<RomEntry> Roms { get; set; } = new List<RomEntry>();

		public class RomEntry
		{
			[Display(Name = "Id")]
			public int Id { get; set; }

			[Display(Name = "Name")]
			public string DisplayName { get; set; }

			[Display(Name = "Md5")]
			public string Md5 { get; set; }

			[Display(Name = "Sha1")]
			public string Sha1 { get; set; }

			[Display(Name = "Version")]
			public string Version { get; set; }

			[Display(Name = "Region")]
			public string Region { get; set; }

			[Display(Name = "Type")]
			public RomTypes RomType { get; set; }
		}
	}

	public class RomEditModel
	{
		public int? Id { get; set; }
		public string SystemCode { get; set; }
		public int GameId { get; set; }
		public string GameName { get; set; }
		public bool CanDelete { get; set; }

		public IEnumerable<SelectListItem> AvailableRomTypes { get; set; } = new List<SelectListItem>();

		[Required]
		[StringLength(255)]
		[Display(Name = "Name")]
		public string Name { get; set; }

		[Required]
		[StringLength(32)]
		[Display(Name = "Md5")]
		public string Md5 { get; set; }

		[Required]
		[StringLength(40)]
		[Display(Name = "Sha1")]
		public string Sha1 { get; set; }

		[Display(Name = "Version")]
		public string Version { get; set; }

		[Required]
		[Display(Name = "Region")]
		public string Region { get; set; }

		[Required]
		[Display(Name = "Type")]
		public RomTypes RomType { get; set; }
	}
}
