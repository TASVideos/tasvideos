﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionPublishModel
	{
		[Display(Name = "Select movie to be obsoleted")]
		public int? MovieToObsolete { get; set; }

		[StringLength(50)]
		[Display(Name = "Branch", Description = "(e.g. \"100%\" or \"princess only\"; \"any%\" can usually be omitted)")]
		public string? Branch { get; set; }

		[StringLength(50)]
		[Display(Name = "Emulator Version")]
		public string? EmulatorVersion { get; set; }

		[Required]
		[Display(Name = "Movie description")]
		public string MovieMarkup { get; set; } = SiteGlobalConstants.DefaultPublicationText;

		[Required]
		[Display(Name = "Movie Filename", Description = "Please follow the convention: xxxv#-yyy where xxx is author name, # is version and yyy is game name. Special characters such as \"&\" and \"/\" and \".\" and spaces must not occur in the filename.")]
		public string MovieFileName { get; set; } = "";

		[Url]
		[Required]
		[Display(Name = "Online-watching URL")]
		[StringLength(500)]
		public string OnlineWatchingUrl { get; set; } = "";

		[Url]
		[Required]
		[Display(Name = "Mirror site URL")]
		[StringLength(500)]
		public string MirrorSiteUrl { get; set; } = "";

		[Required]
		[Display(Name = "Screenshot", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile? Screenshot { get; set; }

		[Display(Name = "Description", Description = "Caption, describe what happens in the screenshot")]
		public string? ScreenshotDescription { get; set; }

		[Display(Name = "Submission description (for quoting, reference, etc)")]
		public string? Markup { get; set; }

		[Display(Name = "System")]
		public string? SystemCode { get; set; }

		[Display(Name = "Region")]
		public string? SystemRegion { get; set; }

		[Display(Name = "Game")]
		public string? Game { get; set; }
		public int GameId { get; set; }

		[Display(Name = "Rom")]
		public string? Rom { get; set; }
		public int RomId { get; set; }

		[Display(Name = "PublicationClass")]
		public string? PublicationClass { get; set; }

		public string? MovieExtension { get; set; }

		// Not used for edit fields
		public string Title { get; set; } = "";
		public int SystemId { get; set; }
		public int? SystemFrameRateId { get; set; }
		public SubmissionStatus Status { get; set; }

		public bool CanPublish => SystemId > 0
			&& SystemFrameRateId.HasValue
			&& GameId > 0
			&& RomId > 0
			&& !string.IsNullOrEmpty(PublicationClass)
			&& Status == SubmissionStatus.PublicationUnderway;
	}
}
