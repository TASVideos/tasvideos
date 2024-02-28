﻿using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Games.Models;

public class GameEditModel
{
	[StringLength(100)]
	[Display(Name = "Display Name")]
	public string DisplayName { get; set; } = "";

	[StringLength(24)]
	public string? Abbreviation { get; set; }

	[StringLength(250)]
	[Display(Name = "Aliases")]
	public string? Aliases { get; set; }

	[StringLength(250)]
	[Display(Name = "Screenshot URL")]
	public string? ScreenshotUrl { get; set; }

	[StringLength(300)]
	[Display(Name = "Game Resources Page")]
	public string? GameResourcesPage { get; set; }

	[Display(Name = "Genres")]
	public IEnumerable<int> Genres { get; set; } = new List<int>();

	[Display(Name = "Groups")]
	public IEnumerable<int> Groups { get; set; } = new List<int>();

	public bool MinorEdit { get; set; }
}
