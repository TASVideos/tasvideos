using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a movie to submit to the submission queue for the submit page
	/// </summary>
	public class SubmissionCreateViewModel
    {
		[Required]
		[Display(Name = "Game Version", Description = "Example: USA")]
		[StringLength(20)]
		public string GameVersion { get; set; }

		public IEnumerable<SelectListItem> GameVersionOptions { get; set; } = new List<SelectListItem>();

		[Required]
		[Display(Name = "Game Name", Description = "Example: Mega Man 2")]
		[StringLength(100)]
		public string GameName { get; set; }

		[Display(Name = "Branch Name", Description = "Example: 100% or princess only; any% can usually be omitted)")]
		[StringLength(50)]
		public string Branch { get; set; }

		[Required]
		[Display(Name = "ROM filename", Description = "Example: Mega Man II (U) [!].nes")]
		[StringLength(100)]
		public string RomName { get; set; }

		[Display(Name = "Emulator and version", Description = "Example: BizHawk 2.2.1")]
		[StringLength(50)]
		public string Emulator { get; set; }

		[Display(Name = "Encode Embedded Link", Description = "Embedded link to a video of your movie, Ex: www.youtube.com/embed/0mregEW6kVU")]
		public string EncodeEmbedLink { get; set; }

		[Display(Name = "Author(s)")]
		[AtLeastOne(ErrorMessage = "A submission must have at least one author")]
		public IList<string> Authors { get; set; } = new List<string>();

		[Required]
		[Display(Name = "Comments and explanations")]
		public string Markup { get; set; }

		[Required]
		[Display(Name = "Movie file", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile MovieFile { get; set; }
	}

	/// <summary>
	/// Represents the result of attempting to parse and save a submission
	/// </summary>
	public class SubmitResult
	{
		public SubmitResult(int id)
		{
			if (id <= 0)
			{
				throw new ArgumentException("Id must be greater than 0");
			}

			Id = id;
			Errors = new List<string>();
		}

		public SubmitResult(string error, int id = 0)
			: this(new[] { error }, id)
		{
		}

		public SubmitResult(IEnumerable<string> errors, int id = 0)
		{
			if ((errors?.Any() ?? false) && id <= 0)
			{
				throw new ArgumentException("Errors must not be null or id must be greater than 0");
			}

			Errors = errors ?? new List<string>();
			Id = 0;
		}

		public IEnumerable<string> Errors { get; }
		public int Id { get; }
		public bool Success => !Errors.Any();
	}

	/// <summary>
	/// Represents an existing submission for the purpose of display
	/// </summary>
	public class SubmissionViewModel
	{
		public int Id { get; set; }
		public bool CanEdit { get; set; }
		public bool IsCatalogged => SystemId.HasValue
			&& SystemFrameRateId.HasValue
			&& GameId.HasValue
			&& RomId.HasValue;

		[Display(Name = "For tier")]
		public string TierName { get; set; }

		[Display(Name = "Console")]
		public string SystemDisplayName { get; set; }

		public string SystemCode { get; set; }

		[Display(Name = "Game name")]
		public string GameName { get; set; }

		[Display(Name = "Game Version")]
		public string GameVersion { get; set; }

		[Display(Name = "ROM filename")]
		public string RomName { get; set; }

		[Display(Name = "Branch")]
		public string Branch { get; set; }

		[Display(Name = "Emulator")]
		public string Emulator { get; set; }

		[Display(Name = "Encode Embed Link")]
		public string EncodeEmbedLink { get; set; }

		[Display(Name = "FrameCount")]
		public int FrameCount { get; set; }

		public double FrameRate { get; set; }

		public int RerecordCount { get; set; }

		[Display(Name = "Author")]
		public IEnumerable<string> Authors { get; set; } = new List<string>();

		[Display(Name = "Submitter")]
		public string Submitter { get; set; }

		[Display(Name = "Submit Date")]
		public DateTime CreateTimestamp { get; set; }

		[Display(Name = "Last Edited")]
		public DateTime LastUpdateTimeStamp { get; set; }

		[Display(Name = "Last Edited by")]
		public string LastUpdateUser { get; set; }

		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		[Display(Name = "Judge")]
		public string Judge { get; set; }

		[Display(Name = "Publisher")]
		public string Publisher { get; set; }

		public string Title { get; set; }

		internal int? SystemId { get; set; }
		internal int? SystemFrameRateId { get; set; }
		internal int? GameId { get; set; }
		internal int? RomId { get; set; }
	}

	// TODO: document
	public class SubmissionEditModel : SubmissionViewModel
	{
		public IEnumerable<SelectListItem> GameVersionOptions { get; set; } = new List<SelectListItem>();

		public string Markup { get; set; }

		[Display(Name = "Revision Message")]
		public string RevisionMessage { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Display(Name = "Status")]
		public IEnumerable<SubmissionStatus> AvailableStatuses { get; set; } = new List<SubmissionStatus>();

		[Display(Name = "Replace Movie file", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile MovieFile { get; set; }

		[Display(Name = "Intended Tier")]
		public int? TierId { get; set; }
		public IEnumerable<SelectListItem> AvailableTiers { get; set; }
	}

	// TODO: document - for reverifying a status can be set
	public class SubmissionStatusValidationModel
	{
		public bool UserIsJudge { get; set; }
		public bool UserIsAuhtorOrSubmitter { get; set; }
		public SubmissionStatus CurrentStatus { get; set; }
		public DateTime CreateDate { get; set; }
	}

	/// <summary>
	/// Filter criteria for submission search
	/// </summary>
	public class SubmissionSearchCriteriaModel
	{
		public int? Limit { get; set; }
		public DateTime? Cutoff { get; set; } // Only submissions submitted after this date
		public string User { get; set; }
		public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = new List<SubmissionStatus>
		{
			SubmissionStatus.New,
			SubmissionStatus.JudgingUnderWay,
			SubmissionStatus.Accepted,
			SubmissionStatus.PublicationUnderway,
			SubmissionStatus.NeedsMoreInfo,
			SubmissionStatus.Delayed
		};
	}

	/// <summary>
	/// A single submisison from a submission search
	/// </summary>
	public class SubmissionListViewModel
	{
		[Display(Name = "Movie name")]
		public string Title => $"{System} {GameName}"
			+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\" " : "")
			+ $" in {Time:g}";

		[Display(Name = "Author")]
		public string Author { get; set; }

		[Display(Name = "Submitted")]
		public DateTime Submitted { get; set; }

		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		public int Id { get; set; }
		public string System { get; set; }
		public string GameName { get; set; }
		public TimeSpan Time { get; set; }
		public string Branch { get; set; }
	}

	/// <summary>
	/// The data necessary to publish a submission
	/// </summary>
	public class SubmissionPublishModel
	{
		[Display(Name = "Select movie to be obsoleted")]
		public int? MovieToObsolete { get; set; }

		public IEnumerable<SelectListItem> AvailableMoviesToObsolete { get; set; }

		[StringLength(50)]
		[Display(Name = "Branch", Description = "(e.g. \"100%\" or \"princess only\"; \"any%\" can usually be omitted)")]
		public string Branch { get; set; }

		[StringLength(50)]
		[Display(Name = "Emulator Version")]
		public string EmulatorVersion { get; set; }

		[Required]
		[Display(Name = "Movie description")]
		public string MovieMarkup { get; set; } = "''[TODO]: describe this movie here''";

		[Required]
		[Display(Name = "Movie Filename", Description = "Please follow the convention: xxxv#-yyy where xxx is author name, # is version and yyy is game name. Special characters such as \"&\" and \"/\" and \".\" and spaces must not occur in the filename.")]
		public string MovieFileName { get; set; }

		[Required]
		[Display(Name = "Online-watching URL")]
		[StringLength(100)]
		public string OnlineWatchingUrl { get; set; }

		[Required]
		[Display(Name = "Mirror site URL")]
		[StringLength(100)]
		public string MirrorSiteUrl { get; set; }

		[Required]
		[Display(Name = "Screenshot", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile Screenshot { get; set; }

		[Required]
		[Display(Name = "Torrent file", Description = "(The tracker URL must be http://tracker.tasvideos.org:6969/announce.)")]
		public IFormFile TorrentFile { get; set; }

		// Not used for edit fields
		public int Id { get; set; }
		public string Title { get; set; }

		[Display(Name = "Submisison description (for quoting, reference, etc)")]
		public string Markup { get; set; }

		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Display(Name = "Region")]
		public string SystemRegion { get; set; }

		[Display(Name = "Game")]
		public string Game { get; set; }
		public int GameId { get; set; }

		[Display(Name = "Rom")]
		public string Rom { get; set; }
		public int RomId { get; set; }

		[Display(Name = "Tier")]
		public string Tier { get; set; }

		public string MovieExtension { get; set; }

		public int SystemId { get; set; }
	}

	public class SubmissionCatalogModel
	{
		public int Id { get; set; }

		[Display(Name = "Rom")]
		public int? RomId { get; set; }

		[Display(Name = "Game")]
		public int? GameId { get; set; }

		[Display(Name = "System")]
		public int? SystemId { get; set; }

		[Display(Name = "System Framerate")]
		public int? SystemFrameRateId { get; set; }

		public IEnumerable<SelectListItem> AvailableRoms { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystemFrameRates { get; set; } = new List<SelectListItem>();
	}
}
