using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

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

		[Required]
		[Display(Name = "Branch Name", Description = "Example: 100% or princess only; any% can usually be omitted)")]
		[StringLength(50)]
		public string BranchName { get; set; }

		// TODO: game id

		[Required]
		[Display(Name = "ROM filename", Description = "Example: Mega Man II (U) [!].nes")]
		[StringLength(100)]
		public string RomName { get; set; }

		[Display(Name = "Emulator and version")]
		[Description("Example: BizHawk 2.2.1")]
		[StringLength(50)]
		public string Emulator { get; set; }

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
	/// Represents an existing submission for the purpose of display
	/// </summary>
	public class SubmissionViewModel
	{
		public int Id { get; set; }
		public string GameName { get; set; }
	}
}
