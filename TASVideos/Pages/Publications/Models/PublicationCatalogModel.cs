﻿using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models;

public class PublicationCatalogModel
{
	public string Title { get; set; } = "";

	[Display(Name = "Game Version")]
	public int GameVersionId { get; set; }

	[Display(Name = "Goal")]
	public int GameGoalId { get; set; }

	[Display(Name = "Game")]
	public int GameId { get; set; }

	[Display(Name = "System")]
	public int SystemId { get; set; }

	[Display(Name = "System Framerate")]
	public int SystemFrameRateId { get; set; }

	public bool MinorEdit { get; set; }
}
