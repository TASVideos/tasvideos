﻿using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionCatalogModel
{
	public string Title { get; set; } = "";

	[Display(Name = "Game Version")]
	public int? GameVersionId { get; set; }

	[Display(Name = "Game")]
	public int? GameId { get; set; }

	[Display(Name = "System")]
	[Required]
	public int? SystemId { get; set; }

	[Display(Name = "System Framerate")]
	[Required]
	public int? SystemFrameRateId { get; set; }

	[Display(Name = "Goal")]
	public int? GameGoalId { get; set; }

	public bool MinorEdit { get; set; }
}
