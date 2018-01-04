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
		public string BranchName { get; set; }

		// TODO: game id

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
			: this (new[] { error }, id)
		{
		}

		public SubmitResult(IEnumerable<string> errors, int id = 0)
		{
			if ((errors?.Any() ?? false) && id <- 0)
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

		public TimeSpan Time
		{
			get
			{
				int seconds = (int) (FrameCount / FrameRate);
				double fractionalSeconds = (FrameCount / FrameRate) - seconds;
				int milliseconds = (int)(Math.Round(fractionalSeconds, 2) * 1000);
				return new TimeSpan(0, 0, 0, seconds, milliseconds);
			}
		}

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
	}

	// TODO: document
	public class SubmissionEditModel : SubmissionViewModel
	{
		// TODO: properties and stuff, and probably don't inherit

		public IEnumerable<SelectListItem> GameVersionOptions { get; set; } = new List<SelectListItem>();

		public string Markup { get; set; }
	}
}
