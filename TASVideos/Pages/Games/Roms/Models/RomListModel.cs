﻿using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Roms.Models;

public class RomListModel
{
	[Display(Name = "Game")]
	public string GameDisplayName { get; set; } = "";

	public IEnumerable<RomEntry> Roms { get; set; } = new List<RomEntry>();

	public class RomEntry
	{
		[Display(Name = "Id")]
		public int Id { get; set; }

		[Display(Name = "Name")]
		public string DisplayName { get; set; } = "";

		[Display(Name = "Md5")]
		public string? Md5 { get; set; }

		[Display(Name = "Sha1")]
		public string? Sha1 { get; set; }

		[Display(Name = "Version")]
		public string? Version { get; set; }

		[Display(Name = "Region")]
		public string? Region { get; set; }

		[Display(Name = "Type")]
		public RomTypes RomType { get; set; }

		[Display(Name = "System")]
		public string SystemCode { get; set; } = "";

		[Display(Name = "Title Override")]
		public string? TitleOverride { get; set; }
	}
}
