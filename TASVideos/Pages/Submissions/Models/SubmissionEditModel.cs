using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionEditModel
	{
		public string Markup { get; set; } = "";

		[StringLength(1000)]
		[Display(Name = "Revision Message")]
		public string? RevisionMessage { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Display(Name = "Replace Movie file", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile? MovieFile { get; set; }

		[Display(Name = "Intended Publication Class")]
		public int? PublicationClassId { get; set; }

		[Display(Name = "Reason")]
		public int? RejectionReason { get; set; }

		public bool IsCataloged => SystemId.HasValue
			&& SystemFrameRateId.HasValue
			&& GameId > 0
			&& RomId > 0;

		[Display(Name = "Start Type")]
		public MovieStartType? StartType { get; set; }

		[Display(Name = "For publication Class")]
		public string? PublicationClass { get; set; }

		[Display(Name = "Console")]
		public string? SystemDisplayName { get; set; }

		public string? SystemCode { get; set; }

		[Display(Name = "Game name")]
		public string? GameName { get; set; }

		[Display(Name = "Game Version")]
		public string? GameVersion { get; set; }

		[Display(Name = "ROM filename")]
		public string? RomName { get; set; }

		[Display(Name = "Branch")]
		public string? Branch { get; set; }

		[Display(Name = "Emulator", Description = "Needs to be a specific version that sync was verified on. Does not necessarily need to be the version used by the author.")]
		public string? Emulator { get; set; }

		[Url]
		[Display(Name = "Encode Embed Link")]
		public string? EncodeEmbedLink { get; set; }

		[Display(Name = "FrameCount")]
		public int FrameCount { get; set; }

		public double FrameRate { get; set; }

		public int RerecordCount { get; set; }

		[Display(Name = "Author")]
		public IEnumerable<string> Authors { get; set; } = new List<string>();

		[Display(Name = "Submitter")]
		public string? Submitter { get; set; }

		[Display(Name = "Submit Date")]
		public DateTime CreateTimestamp { get; set; }

		[Display(Name = "Last Edited")]
		public DateTime LastUpdateTimestamp { get; set; }

		[Display(Name = "Last Edited by")]
		public string? LastUpdateUser { get; set; }

		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		[Display(Name = "Judge")]
		public string? Judge { get; set; }

		[Display(Name = "Publisher")]
		public string? Publisher { get; set; }

		public string? RejectionReasonDisplay { get; set; }

		[Display(Name = "Additional Authors", Description = "Only authors not registered for TASVideos should be listed here. If multiple authors, separate the names with a comma.")]
		public string? AdditionalAuthors { get; set; }

		public string Title { get; set; } = "";

		public bool WarnStartType => StartType.HasValue && StartType != MovieStartType.PowerOn;

		internal int? SystemId { get; set; }
		internal int? SystemFrameRateId { get; set; }
		internal int? GameId { get; set; }
		internal int? RomId { get; set; }
	}
}
